using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Lib;
using System.Runtime.CompilerServices;

namespace SignalRHubs.Hubs
{
    public class ChatHub : Hub
    {
        public async Task UploadStream(IAsyncEnumerable<string> stream, string name)
        {
            Console.WriteLine("Video Stream incomming...");
            
            await foreach (var item in stream)
            {
                _ = Task.Run(() => new StreamThread().CreateDnsThreadAsync(name, item));
                // Wanna directly send to Azure inputUrl
            }
        }
        public async IAsyncEnumerable<int> Counter(
            int count,
            int delay,
            [EnumeratorCancellation]
                CancellationToken cancellationToken)
        {
            Console.WriteLine("Video Streaming Started...");
            for (var i = 0; i < count; i++)
            {
                // Check the cancellation token regularly so that the server will stop
                // producing items if the client disconnects.
                cancellationToken.ThrowIfCancellationRequested();

                yield return i;
                Console.WriteLine("returned value {0}", i);

                // Use the cancellationToken in other APIs that accept cancellation
                // tokens so the cancellation can flow down to them.
                await Task.Delay(delay, cancellationToken);
            }
        }

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