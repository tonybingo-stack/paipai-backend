using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace SignalRHubs.Hubs
{
    public class ChatHub : Hub
    {
        public ChatHub()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public override async Task OnConnectedAsync()
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, Context.User.Identity.GetUserId().ToString());
        //    await base.OnConnectedAsync();
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="exception"></param>
        ///// <returns></returns>
        //public override async Task OnDisconnectedAsync(Exception? exception)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, Context.User.Identity.GetUserId().ToString());
        //    await base.OnDisconnectedAsync(exception);
        //}

        ///// <summary>
        ///// Send message to connected users
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //public async Task SendAsync(Message message)
        //{
        //    //await Clients.All.SendAsync("ReceiveMessage", message);
        //    ////await Clients.All.SendAsync("sendMessage", message);
        //}
        //public async Task BroadcastMessage(string message)
        //{
        //    await Clients.All.ReceiveMessage(GetMessageToSend(message));
        //}

        //public async Task SendToOthers(string message)
        //{
        //    await Clients.Others.ReceiveMessage(GetMessageToSend(message));
        //}

        //public Task SendToCaller(string message)
        //{
        //    throw new Exception("You cannot send messages to yourself.");
        //}

        //public async Task SendToIndividual(string connectionId, string message)
        //{
        //    await Clients.Client(connectionId).ReceiveMessage(GetMessageToSend(message));
        //}

        //public async Task SendToGroup(string groupName, string message)
        //{
        //    await Clients.Group(groupName).ReceiveMessage(GetMessageToSend(message));
        //}

        //public async Task AddUserToGroup(string groupName)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        //    await Clients.Caller.ReceiveMessage($"Current user added to {groupName} group");
        //    await Clients.Others.ReceiveMessage($"User {Context.ConnectionId} added to {groupName} group");
        //}
        //public async Task RemoveUserFromGroup(string groupName)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        //    await Clients.Caller.ReceiveMessage($"Current user removed from {groupName} group");
        //    await Clients.Others.ReceiveMessage($"User {Context.ConnectionId} removed from {groupName} group");
        //}
        //public async Task BroadcastStream(IAsyncEnumerable<string> stream)
        //{
        //    await foreach (var item in stream)
        //    {
        //        await Clients.Caller.ReceiveMessage($"Server received {item}");
        //    }
        //}

        //public async IAsyncEnumerable<string> TriggerStream(
        //    int jobsCount,
        //    [EnumeratorCancellation]
        //    CancellationToken cancellationToken)
        //{
        //    for (var i = 0; i < jobsCount; i++)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();
        //        yield return $"Job {i} executed succesfully";
        //        await Task.Delay(1000, cancellationToken);
        //    }
        //}

        //public override async Task OnConnectedAsync()
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, "HubUsers");

        //    await base.OnConnectedAsync();
        //}

        //public override async Task OnDisconnectedAsync(Exception? exception)
        //{
        //    await Groups.RemoveFromGroupAsync(Context.ConnectionId, "HubUsers");
        //    await base.OnDisconnectedAsync(exception);
        //}

        //public string GetMessageToSend(string originalMessage)
        //{
        //    return $"UserName: {Context.User.Identity.Name} User connection id: {Context.ConnectionId}. Message: {originalMessage}";
        //}
        //public string GetConnectionID()
        //{
        //    return Context.ConnectionId;
        //}


        public void BroadcastMessage(string name, string message)
        {
            Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public void Echo(string name, string message)
        {
            Clients.Client(Context.ConnectionId).SendAsync("echo", name, message + " (echo from server)");
        }

        public async Task JoinGroup(string name, string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("echo", "_SYSTEM_", $"{name} joined {groupName} with connectionId {Context.ConnectionId}");
        }

        public async Task LeaveGroup(string name, string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            await Clients.Client(Context.ConnectionId).SendAsync("echo", "_SYSTEM_", $"{name} leaved {groupName}");
            await Clients.Group(groupName).SendAsync("echo", "_SYSTEM_", $"{name} leaved {groupName}");
        }

        public void SendGroup(string name, string groupName, string message)
        {
            Clients.Group(groupName).SendAsync("echo", name, message);
        }

        public void SendGroups(string name, IReadOnlyList<string> groups, string message)
        {
            Clients.Groups(groups).SendAsync("echo", name, message);
        }

        public void SendGroupExcept(string name, string groupName, IReadOnlyList<string> connectionIdExcept, string message)
        {
            Clients.GroupExcept(groupName, connectionIdExcept).SendAsync("echo", name, message);
        }

        public void SendUser(string name, string userId, string message)
        {
            //Console.WriteLine(name+":"+ userId+":"+ message);
            Clients.User(userId).SendAsync("echo", name, message);
        }

        public void SendUsers(string name, IReadOnlyList<string> userIds, string message)
        {
            Clients.Users(userIds).SendAsync("echo", name, message);
        }
    }
}