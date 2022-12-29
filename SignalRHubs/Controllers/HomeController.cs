using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;

namespace SignalRHubs.Controllers
{
    public class HomeController : ApiBaseController
    {
        private readonly IMapper _mapper;
        private readonly IHomeService _homeService;
        private readonly IUserService _userService;
        public HomeController(IHomeService service, IUserService userService, IMapper mapper): base(userService)
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
        [ProducesResponseType(400)]
        [HttpPost("/create-new-community")]
        public async Task<IActionResult> CreateCommunity([FromForm] CommunityModel model)
        {
            //Guid UserId = await _userService.GetIdByUserName(UserName);

            Community entity = _mapper.Map<Community>(model);
            entity.CommunityOwnerId = await UserId;

            return Ok(await _homeService.CreateCommunity(entity));
            //return Ok(myuser);
        }

        /// <summary>
        /// Create New Channels
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/create-new-channel")]
        public async Task<IActionResult> CreateChannel([FromForm] ChannelModel model)
        {
            Channel entity = _mapper.Map<Channel>(model);
            return Ok(await _homeService.CreateChannel(entity));
            //return Ok(myuser);
        }

        /// <summary>
        /// Test Controller
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpGet("/test")]
        public async Task<Guid> Test()
        {
            return await UserId;
        }
    }
}
