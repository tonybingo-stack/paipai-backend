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
        /// Send Friend invite to user
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("/friend/invite")]
        public async Task<IActionResult> SendInvitation([Required][FromForm] string username)
        {
            await _hubContext.Clients.User(username).SendAsync("invite", UserName);
            //await _hubContext.Clients.User(username).SendAsync("echo", UserName, Emoji.GrinningFace);
            // add username to my friend list
            await _userService.AddUserToFriendList(UserName, username);
            // save message to cache
            //string encode = Utils.Base64Encode(UserName, username);

            //Message message = new Message();
            //message.Id = Guid.NewGuid();
            //message.SenderUserName = UserName;
            //message.ReceiverUserName = username;
            //message.Content = Emoji.GrinningFace;
            //message.isDeleted = false;
            //message.CreatedAt = DateTime.Now;

            //List<Message> data = new List<Message>();
            //string? serializedData = null;

            //data.Insert(0, message);
            //serializedData = JsonConvert.SerializeObject(data);
            //var dataAsByteArray = Encoding.UTF8.GetBytes(serializedData);
            //await _cache.SetAsync($"Message:" + encode, dataAsByteArray);

            // create chat card for this 2 user
            //await _chatService.RefreshChatCard(UserName, username, message);

            return Ok("ok");

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
