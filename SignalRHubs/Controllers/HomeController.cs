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
        public async Task<IActionResult> CreateCommunity(Community community)
        {
            return Ok(await _homeService.CreateCommunity(community));
            //return Ok(myuser);
        }
    }
}
