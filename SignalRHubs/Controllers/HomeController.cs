using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Entities;
using SignalRHubs.Interfaces.Services;

namespace SignalRHubs.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        public HomeController(IHomeService service)
        {
            this._homeService = service;
        }
        /// <summary>
        /// Create New Community
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/createcommunity")]
        public async Task<IActionResult> CreateCommunity(Community entity)
        {
            return Ok(await _homeService.CreateCommunity(entity));
            //return Ok(myuser);
        }

        /// <summary>
        /// Create New Channels
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/createchannel")]
        public async Task<IActionResult> CreateChannel(Channel entity)
        {
            return Ok(await _homeService.CreateChannel(entity));
            //return Ok(myuser);
        }
    }
}
