using Microsoft.AspNetCore.Mvc;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;

namespace SignalRHubs.Controllers
{
    public class FriendController : ApiBaseController
    {
        private readonly IUserService _userService;
        public FriendController(IUserService userService) : base(userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// Get all friends
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<UserViewModel>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("/friend/friends")]
        public async Task<IActionResult> GetAllFriends()
        {
            return Ok(await _userService.GetAllFriends(UserName));
        }
        /// <summary>
        /// Add user to friend list
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/friend/add")]
        public async Task<IActionResult> AddUserToFriendList([Required][FromForm]string username)
        {
            return Ok(await _userService.AddUserToFriendList(UserName, username));
        }
        /// <summary>
        /// Remove user From friend list
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpDelete("/friend/delete")]
        public async Task<IActionResult> RemoveUserFromFriend([Required][FromForm] string username)
        {
            return Ok(await _userService.RemoveUserFromFriend(UserName, username));
        }
        /// <summary>
        /// Block User
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPut("/friend/block")]
        public async Task<IActionResult> BlockUser([Required][FromForm] string username)
        {
            return Ok(await _userService.BlockUser(UserName, username));
        }
    }
}
