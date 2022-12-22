using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Entities;
using SignalRHubs.Extensions;
using SignalRHubs.Interfaces.Services;

namespace SignalRHubs.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUserService _userService;

        public ChatHub(IUserService service)
        {
            _userService = service;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            Guid UserId = await _userService.GetIdByUserName(Context.User.Identity.GetUserName());

            await Groups.AddToGroupAsync(Context.ConnectionId, UserId.ToString());
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Guid UserId = await _userService.GetIdByUserName(Context.User.Identity.GetUserName());

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UserId.ToString());
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Send message to connected users
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(Message message)
        {
            await Clients.All.SendAsync("sendMessage", message);
        }
    }
}