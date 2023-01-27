using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RedisCacheForPaiPai;
using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Controllers
{
    public class CommunityController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConfiguration _iconfiguration;
        private RedisConnection _redisConnection;

        public CommunityController(IHomeService service, IUserService userService, IMapper mapper, IHubContext<ChatHub> hubContext, IConfiguration iconfiguration) : base(userService)
        {
            _homeService = service;
            _mapper = mapper;
            _hubContext = hubContext;
            _iconfiguration = iconfiguration;
        }

        /// <summary>
        /// Test redis
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/community/numberofuser")]
        public async Task<IActionResult> GetUserNumberOfCommunity([FromQuery][Required] Guid communityId)
        {
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);

            //var res1= (await _redisConnection.BasicRetryAsync(async (db) => await db.ExecuteAsync("PING"))).ToString();
            //await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync("test", "great"));
            var res= await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(communityId.ToString()));
            if (res.IsNull == true)
            {
                return Ok("Key doesn't exist in Redis Cache. Try again in another community.");
            }
            return Ok(res.ToString());
        }
        /// <summary>
        /// Create New Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-community")]
        public async Task<IActionResult> CreateCommunity([FromForm] CommunityModel model)
        {
            Community entity = _mapper.Map<Community>(model);
            entity.CommunityName = entity.CommunityName.Replace("'", "''");
            entity.CommunityOwnerName = UserName;
            if(entity.CommunityDescription!=null) entity.CommunityDescription = entity.CommunityDescription.Replace("'", "''");

            var response = await _homeService.CreateCommunity(entity);

            // Create Redis cache for this community
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);

            await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(response.ToString(), 1));
            await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(response.ToString()+"_old", 1));
            //var res = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(communityId.ToString()));
            //GlobalModule.NumberOfUsers[response.ToString()] = 1;
            //GlobalModule.NumberOfPosts[response.ToString()] = 0;
            //GlobalModule.NumberOfActiveUser[response.ToString()] = 1;

            return Ok(response);
        }
        /// <summary>
        /// Update Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/community")]
        public async Task<IActionResult> UpdateCommunity([FromForm] CommunityUpdateModel model)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, model.Id);
            if (m.UserRole > 0) return BadRequest("UserRole is not enough to perform this action!");

            if (model.Id == null) return BadRequest("Community ID is required!");
            if (model.CommunityName == null && model.CommunityDescription == null && model.CommunityType == null && model.ForegroundImage == null && model.BackgroundImage == null) return BadRequest("At least one field is required!");
            
            Community entity = _mapper.Map<Community>(model);
            if (entity.CommunityName != null) entity.CommunityName = entity.CommunityName.Replace("'", "''");
            if (entity.CommunityDescription != null) entity.CommunityDescription = entity.CommunityDescription.Replace("'", "''");
            entity.CommunityOwnerName = UserName;
            entity.UpdatedAt = DateTime.Now;

            return Ok(await _homeService.UpdateCommunity(entity));
        }
        /// <summary>
        /// Delete Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/community")]
        public async Task<IActionResult> DeleteCommunity([Required] Guid id)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, id);
            if (m.UserRole > 0) return BadRequest("UserRole is not enough to perform this action!");
            var res = await _homeService.DeleteCommunity(id);

            // Clear Redis Cache for this community
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);
            await _redisConnection.BasicRetryAsync(async (db) => db.KeyDelete(id.ToString()));
            await _redisConnection.BasicRetryAsync(async (db) => db.KeyDelete(id.ToString()+"_old"));

            return Ok(res);
        }
        
        /// <summary>
        /// Get all communities user joined
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CommunityViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/joined-community")]
        public async Task<IActionResult> GetAllJoinedCommunity()
        {
            var res = await _homeService.GetJoinedCommunity(UserName);
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);

            foreach (var item in res)
            {
                var num = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(item.Id.ToString()));
                item.NumberOfUsers = Int32.Parse(num.ToString());
            }
            return Ok(res);
        }
        /// <summary>
        /// Get communities for feed page
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CommunityViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/community/feed")]
        public async Task<IActionResult> GetCommunityForFeed([Required][FromQuery]int offset)
        {
            var res = await _homeService.GetCommunityForFeed(offset);
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);

            foreach (var item in res)
            {
                var num = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(item.Id.ToString()));
                item.NumberOfUsers = Int32.Parse(num.ToString());
            }
            return Ok(res);
        }
        /// <summary>
        /// Join a community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/community/join")]
        public async Task<IActionResult> JoinCommunity([FromForm][Required] Guid communityId)
        {
            //GlobalModule.NumberOfUsers[communityId.ToString()] = (int)GlobalModule.NumberOfUsers[communityId.ToString()] + 1;
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);
            var cur = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(communityId.ToString()));
            var old = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(communityId.ToString() + "_old"));
            if (cur.IsNull || old.IsNull) return BadRequest("Redis Cache Exception: No key exist!");

            var res = await _homeService.JoinCommunity(UserName, communityId);

            // if old<cur+100 then update db
            if (Int32.Parse(old.ToString()) < Int32.Parse(cur.ToString()) + 100)
            {
                await _homeService.UpdateUserNumberOfCommunity(Int32.Parse(cur.ToString()), communityId);
                await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(communityId.ToString()+"_old", Int32.Parse(cur.ToString()) + 1));
            }
            await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(communityId.ToString(), Int32.Parse(cur.ToString())+1));
            return Ok(res);
        }
        /// <summary>
        /// Exit from community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/community/exit")]
        public async Task<IActionResult> ExitCommunity([FromForm][Required] Guid communityId)
        {
            //GlobalModule.NumberOfUsers[communityId.ToString()] = (int)GlobalModule.NumberOfUsers[communityId.ToString()] - 1;
            _redisConnection = await RedisConnection.InitializeAsync(connectionString: _iconfiguration["ConnectionStrings:RedisCache"]);

            var cur = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(communityId.ToString()));
            var old = await _redisConnection.BasicRetryAsync(async (db) => await db.StringGetAsync(communityId.ToString() + "_old"));
            if (cur.IsNull || old.IsNull) return BadRequest("Redis Cache Exception: No key exist!");

            var res = await _homeService.ExitCommunity(UserName, communityId);

            await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(communityId.ToString(), Int32.Parse(cur.ToString()) - 1));
            // if old>cur-100 then update db
            if (Int32.Parse(old.ToString()) > Int32.Parse(cur.ToString()) - 100)
            {
                await _homeService.UpdateUserNumberOfCommunity(Int32.Parse(cur.ToString()), communityId);
                await _redisConnection.BasicRetryAsync(async (db) => await db.StringSetAsync(communityId.ToString() + "_old", Int32.Parse(cur.ToString())));
            }
            return Ok(res);
        }
        /// <summary>
        /// Provide admin role to joined user
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/community/admin")]
        public async Task<IActionResult> ProvideAdminRole([FromForm][Required] string username, [FromForm][Required] Guid communityId)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, communityId);
            if (m == null || m.UserRole > 0) return BadRequest("You are not super admin of this community!");
            m = await _homeService.GetUserRole(username, communityId);
            if (m == null) return BadRequest("User not joined in this community!");
            if (m.UserRole < 2) return BadRequest("User is already admin of this community!");
            // Check User Role

            return Ok(await _homeService.AddAdmin(username, communityId));
        }
        /// <summary>
        /// Cancel admin role to joined user
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/community/admin")]
        public async Task<IActionResult> CancelAdminRole([FromForm][Required] string username, [FromForm][Required] Guid communityId)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, communityId);
            if (m == null || m.UserRole > 0) return BadRequest("You are not super admin of this community!");
            m = await _homeService.GetUserRole(username, communityId);
            if (m == null) return BadRequest("User not joined in this community!");
            if (m.UserRole > 1) return BadRequest("User is not admin of this community!");
            // Check User Role

            return Ok(await _homeService.RemoveAdmin(username, communityId));
        }
    
 
    }
}
