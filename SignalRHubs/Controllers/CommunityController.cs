using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        public CommunityController(IHomeService service, IUserService userService, IMapper mapper, IHubContext<ChatHub> hubContext) : base(userService)
        {
            _homeService = service;
            _mapper = mapper;
            _hubContext = hubContext;
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

            GlobalModule.NumberOfUsers[response.ToString()] = 1;
            GlobalModule.NumberOfPosts[response.ToString()] = 0;
            GlobalModule.NumberOfActiveUser[response.ToString()] = 1;

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

            // Update referenced table
            return Ok(await _homeService.DeleteCommunity(id));
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
            return Ok(await _homeService.GetJoinedCommunity(UserName));
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
            GlobalModule.NumberOfUsers[communityId.ToString()] = (int)GlobalModule.NumberOfUsers[communityId.ToString()] + 1;
            return Ok(await _homeService.JoinCommunity(UserName, communityId));
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
            GlobalModule.NumberOfUsers[communityId.ToString()] = (int)GlobalModule.NumberOfUsers[communityId.ToString()] - 1;
            return Ok(await _homeService.ExitCommunity(UserName, communityId));
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
