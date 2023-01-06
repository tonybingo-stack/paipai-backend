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
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-community")]
        public async Task<IActionResult> CreateCommunity([FromForm] CommunityModel model)
        {
            Community entity = _mapper.Map<Community>(model);
            entity.CommunityOwnerId = await UserId;

            return Ok(await _homeService.CreateCommunity(entity));
            //return Ok(myuser);
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
        /// Create New Channels
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(NotFoundResult), 400)]
        [HttpPost("/create-new-channel")]
        public async Task<IActionResult> CreateChannel([FromForm] ChannelModel model)
        {
            Channel entity = _mapper.Map<Channel>(model);
            return Ok(await _homeService.CreateChannel(entity));
            //return Ok(myuser);
        }

    }
}
