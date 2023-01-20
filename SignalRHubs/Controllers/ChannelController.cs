using Microsoft.AspNetCore.Mvc;
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
    public class ChannelController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IHubContext<ChatHub> _hubContext;
        public ChannelController(IHomeService service, IUserService userService, IMapper mapper, IHubContext<ChatHub> hubContext) : base(userService)
        {
            _homeService = service;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        /// <summary> 
        /// Create New Channels
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-channel")]
        public async Task<IActionResult> CreateChannel([FromForm] ChannelModel model)
        {
            // check user role
            CommunityMember m = await _homeService.GetUserRole(UserName, model.ChannelCommunityId);
            if (m == null || m.UserRole > 1) return BadRequest("User Role is not enough to perform this action!");

            Channel entity = _mapper.Map<Channel>(model);
            entity.ChannelOwnerName = UserName;
            entity.ChannelName = entity.ChannelName.Replace("'", "''");
            if (entity.ChannelDescription != null) entity.ChannelDescription = entity.ChannelDescription.Replace("'", "''");
            var res = await _homeService.CreateChannel(entity);

            await _hubContext.Groups.AddToGroupAsync(UserName, res.ToString());
            await _hubContext.Clients.Group(res.ToString()).SendAsync("echo", "_SYSTEM_", $"{UserName} joined Channel {res}");

            return Ok(res);
        }

        /// <summary> 
        /// Get channels of Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(ChannelViewModel), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/channels")]
        public async Task<IActionResult> GetAllChannels([FromQuery][Required] Guid communityID)
        {
            return Ok(await _homeService.GetAllChannels(communityID));
        }
        /// <summary> 
        /// Update channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPut("/channel")]
        public async Task<IActionResult> UpdateChannel([FromForm] ChannelUpdateModel model)
        {
            // Check User Role
            // Get current owner name from db, compare it with UserName.
            Channel c = await _homeService.GetChannelById(model.ChannelId);
            if (c.ChannelOwnerName != UserName) return BadRequest("You are not owner of this channel!");
            if (model.ChannelName == null && model.ChannelDescription == null) return BadRequest("Name or Description is required!");

            if (model.ChannelDescription != null) model.ChannelDescription = model.ChannelDescription.Replace("'", "''");
            if (model.ChannelName != null) model.ChannelName = model.ChannelName.Replace("'", "''");

            return Ok(await _homeService.UpdateChannel(model));
        }
        /// <summary>
        /// Delete Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/channel")]
        public async Task<IActionResult> DeleteChannel([Required] Guid channelId)
        {
            // Check User Role
            Community m = await _homeService.GetCommunityByChannelId(channelId);
            if (m == null) return BadRequest("Channel or Community not exist!");

            if (m.CommunityOwnerName != UserName) return BadRequest("Only owner of community can delete this channel!");

            return Ok(await _homeService.DeleteChannel(channelId));
        }
        /// <summary>
        /// Join Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/channel/join")]
        public async Task<IActionResult> JoinChannel([Required] Guid channelId)
        {
            // Check User Role
            await _hubContext.Groups.AddToGroupAsync(UserName, channelId.ToString());
            await _hubContext.Clients.Group(channelId.ToString()).SendAsync("echo", "_SYSTEM_", $"{UserName} joined Channel {channelId}");

            return Ok(await _homeService.JoinChannel(UserName, channelId));
        }
        /// <summary>
        /// Exit Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/channel/join")]
        public async Task<IActionResult> ExitChannel([Required] Guid channelId)
        {
            // Check User Role
            await _hubContext.Groups.RemoveFromGroupAsync(UserName, channelId.ToString());
            await _hubContext.Clients.Group(channelId.ToString()).SendAsync("echo", "_SYSTEM_", $"{UserName} leaved Channel {channelId}");

            return Ok(await _homeService.ExitChannel(UserName, channelId));
        }
    }
}
