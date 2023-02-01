using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SignalRHubs.Controllers
{
    public class CommunityController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IConfiguration _iconfiguration;
        //private RedisConnection _redisConnection;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions expiration;

        public CommunityController(IHomeService service, IUserService userService, IMapper mapper, IHubContext<ChatHub> hubContext, 
            IConfiguration iconfiguration, IDistributedCache cache) : base(userService)
        {
            _homeService = service;
            _mapper = mapper;
            _hubContext = hubContext;
            _iconfiguration = iconfiguration;
            _cache = cache;
            expiration = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(100),
                SlidingExpiration = TimeSpan.FromDays(100)
            };
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
            string key = communityId.ToString().Replace("-", "");
            var res = await _cache.GetAsync(key);
            if ((res?.Count() ?? 0) > 0)
            {
                return Ok(Encoding.UTF8.GetString(res));
            }
            return Ok("Key doesn't exist in Redis Cache. Try again in another community.");

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
            entity.CommunityName = entity.CommunityName;
            entity.CommunityOwnerName = UserName;
            if(entity.CommunityDescription!=null) entity.CommunityDescription = entity.CommunityDescription;

            var response = await _homeService.CreateCommunity(entity);

            // Create Redis cache for this community
            await _cache.SetAsync(response.ToString().Replace("-", ""), Encoding.UTF8.GetBytes("1"), expiration);
            await _cache.SetAsync(response.ToString().Replace("-", "") + "_old", Encoding.UTF8.GetBytes("1"), expiration);

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
            if (entity.CommunityName != null) entity.CommunityName = entity.CommunityName;
            if (entity.CommunityDescription != null) entity.CommunityDescription = entity.CommunityDescription;
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

            await _cache.RemoveAsync(id.ToString().Replace("-", ""));
            await _cache.RemoveAsync(id.ToString().Replace("-", "") + "_old");

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

            foreach (var item in res)
            {
                var num = await _cache.GetAsync(item.Id.ToString().Replace("-", ""));
                item.NumberOfUsers = Int32.Parse(Encoding.UTF8.GetString(num));
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

            foreach (var item in res)
            {
                var num = await _cache.GetAsync(item.Id.ToString().Replace("-", ""));
                item.NumberOfUsers = Int32.Parse(Encoding.UTF8.GetString(num));
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
            var cur = await _cache.GetAsync(communityId.ToString().Replace("-", ""));
            var old = await _cache.GetAsync(communityId.ToString().Replace("-", "") + "_old");
            if ((cur?.Count() ?? 0) == 0 || (old?.Count() ?? 0) == 0) return BadRequest("Redis Cache Exception: No key exist!");

            var res = await _homeService.JoinCommunity(UserName, communityId);

            // if old<cur+100 then update db
            if (Int32.Parse(Encoding.UTF8.GetString(old)) < Int32.Parse(Encoding.UTF8.GetString(cur)) + 100)
            {
                await _homeService.UpdateUserNumberOfCommunity(Int32.Parse(Encoding.UTF8.GetString(cur)), communityId);
                await _cache.SetAsync(communityId.ToString().Replace("-", "") + "_old", Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(cur)), expiration);
            }
            await _cache.SetAsync(communityId.ToString().Replace("-", ""), Encoding.UTF8.GetBytes((Int32.Parse(Encoding.UTF8.GetString(cur))+1).ToString()), expiration);

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
            var cur = await _cache.GetAsync(communityId.ToString().Replace("-", ""));
            var old = await _cache.GetAsync(communityId.ToString().Replace("-", "") + "_old");
            if ((cur?.Count() ?? 0) == 0 || (old?.Count() ?? 0) == 0) return BadRequest("Redis Cache Exception: No key exist!");

            var res = await _homeService.ExitCommunity(UserName, communityId);

            // if old>cur-100 then update db
            if (Int32.Parse(Encoding.UTF8.GetString(old)) > Int32.Parse(Encoding.UTF8.GetString(cur)) - 100)
            {
                await _homeService.UpdateUserNumberOfCommunity(Int32.Parse(Encoding.UTF8.GetString(cur)), communityId);
                await _cache.SetAsync(communityId.ToString().Replace("-", "") + "_old", Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(cur)), expiration);
            }
            await _cache.SetAsync(communityId.ToString().Replace("-", ""), Encoding.UTF8.GetBytes((Int32.Parse(Encoding.UTF8.GetString(cur)) - 1).ToString()), expiration);

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
