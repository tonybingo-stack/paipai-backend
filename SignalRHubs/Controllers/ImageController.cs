using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Interfaces.Services;

namespace SignalRHubs.Controllers
{
    public class ImageController : ApiBaseController
    {
        private readonly IUserService _userService;
        private readonly IHomeService _homeService;

        public ImageController(IHomeService homeService, IUserService userService) : base(userService)
        {
            _userService = userService;
            _homeService = homeService;
        }
        /// <summary>
        /// Create or Update User Avatar
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("/user/avatar")]
        public async Task<IActionResult> UpdateUserAvatar([FromForm] string url)
        {
            //if (CheckURLValid(url)) return BadRequest("Invalid url");
            return Ok(await _userService.UpdateUserAvatar(url, UserName));
        }
        //public static bool CheckURLValid(string strURL)
        //{
        //    Uri uriResult;
        //    return Uri.TryCreate(strURL, UriKind.RelativeOrAbsolute, out uriResult);
        //}
        /// <summary>
        /// Create or Update Community Avatar
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("/community/foreground")]
        public async Task<IActionResult> CommunityAvatar([FromForm] Guid communityid, [FromForm] string url)
        {
            //if (CheckURLValid(url)) return BadRequest("Invalid url");
            return Ok(await _homeService.UpdateCommunityAvatar(communityid, url));
        }
        /// <summary>
        /// Create or Update Community Background Image
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("/community/background")]
        public async Task<IActionResult> CommunityBkGround([FromForm] Guid communityid, [FromForm] string url)
        {
            //if (CheckURLValid(url)) return BadRequest("Invalid url");
            return Ok(await _homeService.UpdateCommunityBackGround(communityid, url));
        }
    }
}
