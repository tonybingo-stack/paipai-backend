using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Hubs;

namespace SignalRHubs.Controllers
{
    public class StreamController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        public StreamController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }
        /// <summary>
        /// start stream
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(400)]
        [HttpGet("/stream/start")]
        public async Task<IActionResult> StartStream()
        {
            return Ok("stream");
        }
    }
}
