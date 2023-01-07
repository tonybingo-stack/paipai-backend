using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

namespace SignalRHubs.Controllers
{
    public class CommunityController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IUserService _userService;
        public CommunityController(IHomeService service, IUserService userService, IMapper mapper): base(userService)
        {
            _homeService = service;
            _userService = userService;
            _mapper = mapper;
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
            entity.CommunityOwnerId = await UserId;

            return Ok(await _homeService.CreateCommunity(entity));
        }
        /// <summary>
        /// Get all communities
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<CommunityViewModel>), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpGet("/community")]
        public async Task<IActionResult> GetAllCommunity()
        {
            return Ok(await _homeService.GetCommunity(await UserId));
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
            Community entity = _mapper.Map<Community>(model);
            entity.CommunityOwnerId = await UserId;
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
        public async Task<IActionResult> DeleteCommunity(Guid id)
        {
            return Ok(await _homeService.DeleteCommunity(id));
        }
        /// <summary> 
        /// Create New Channels
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid),200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-channel")]
        public async Task<IActionResult> CreateChannel([FromForm] ChannelModel model)
        {
            Channel entity = _mapper.Map<Channel>(model);
            return Ok(await _homeService.CreateChannel(entity));
        }
        /// <summary> 
        /// Get channels of Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(ChannelViewModel),200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/channels")]
        public async Task<IActionResult> GetAllChannels([FromForm] Guid communityID)
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
            return Ok(await _homeService.UpdateChannel(model));
        }
        /// <summary>
        /// Delete Channel
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpDelete("/channel")]
        public async Task<IActionResult> DeleteChannel(Guid channelId)
        {
            return Ok(await _homeService.DeleteChannel(channelId));
        }
    }
}
