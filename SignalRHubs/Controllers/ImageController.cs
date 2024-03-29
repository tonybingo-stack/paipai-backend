﻿using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

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
        public async Task<IActionResult> UpdateUserAvatar([FromForm][Required] string url)
        {
            //if (CheckURLValid(url)) return BadRequest("Invalid url");
            return Ok(await _userService.UpdateUserAvatar(url, UserName));
        }
        /// <summary>
        /// Create or Update User Background
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("/user/background")]
        public async Task<IActionResult> UpdateUserBackground([FromForm][Required] string url)
        {
            //if (CheckURLValid(url)) return BadRequest("Invalid url");
            return Ok(await _userService.UpdateUserBackground(url, UserName));
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
        public async Task<IActionResult> CommunityAvatar([FromForm][Required] Guid communityid, [FromForm][Required] string url)
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
        public async Task<IActionResult> CommunityBkGround([FromForm][Required] Guid communityid, [FromForm][Required] string url)
        {
            //if (CheckURLValid(url)) return BadRequest("Invalid url");
            return Ok(await _homeService.UpdateCommunityBackGround(communityid, url));
        }
    }
}
