using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Entities;
using SignalRHubs.Extensions;

namespace SignalRHubs.Hubs
{
    public class ChatHub : Hub
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.GetUserId().ToString());
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.GetUserId().ToString());
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