using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SignalRHubs.Entities;
using SignalRHubs.Hubs;
using SignalRHubs.Interfaces.Services;
using SignalRHubs.Lib;
using SignalRHubs.Models;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SignalRHubs.Controllers
{
    public class FriendController : ApiBaseController
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IDistributedCache _cache;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        public FriendController(IUserService userService, IChatService chatService, IHubContext<ChatHub> hubContext, IDistributedCache cache) : base(userService)
        {
            _userService = userService;
            _hubContext = hubContext;
            _cache = cache;
            _chatService = chatService;
        }
        /// <summary>
        /// Get all friends
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<FriendViewModel>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("/friend/friends")]
        public async Task<IActionResult> GetAllFriends()
        {
            return Ok(await _userService.GetAllFriends(UserName));
        }
        /// <summary>
        /// Accept user's invitation
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/friend/accept")]
        public async Task<IActionResult> AcceptUserInvite([Required][FromForm]string username)
        {
            // Check if invitation exists.
            var result = await _chatService.CheckUserFriendShip(username, UserName);
            if (result != "pending") return BadRequest("No pending invitation exists.");

            return Ok(await _userService.AcceptUserInvite(UserName, username));
        }
        /// <summary>
        /// Send invitation
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/friend/invite")]
        public async Task<IActionResult> SendInvitation([Required][FromForm] string username)
        {
            await _hubContext.Clients.User(username).SendAsync("invite", UserName);
            // add username to my friend list
            return Ok(await _userService.SendInvitation(UserName, username));
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
