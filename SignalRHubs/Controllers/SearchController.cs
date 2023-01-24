using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Controllers
{
    public class SearchController : ApiBaseController
    {
        private readonly IHomeService _service;
        public SearchController(IUserService userService, IHomeService service) : base(userService)
        {
            _service = service;
        }
        /// <summary>
        /// Search for Community, Users, Posts, Events
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(SearchResultViewModel), 200)]
        [ProducesResponseType(400)]
        [HttpGet("/search")]
        public async Task<IActionResult> GetSearchResult([Required][FromQuery] string text)
        {
            return Ok(await _service.GetSearchResult(text));
        }
    }
}
