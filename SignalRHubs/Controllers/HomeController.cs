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
        public HomeController(IHomeService service, IMapper mapper)
        {
            this._homeService = service;
            _mapper = mapper;
        }
        /// <summary>
        /// Create New Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/create-new-community")]
        public async Task<IActionResult> CreateCommunity(CommunityModel model)
        {
            Community entity = _mapper.Map<Community>(model);
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
        public async Task<IActionResult> CreateChannel(ChannelModel model)
        {
            Channel entity = _mapper.Map<Channel>(model);
            return Ok(await _homeService.CreateChannel(entity));
            //return Ok(myuser);
        }
    }
}
