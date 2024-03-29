﻿using Microsoft.AspNetCore.SignalR;
using SignalRHubs.Lib;
using System.Runtime.CompilerServices;
using DotNetPusher.Pushers;
using DotNetPusher.Encoders;
using DotNetPusher.VideoPackets;
using System.Runtime.InteropServices;
using System.Text;

namespace SignalRHubs.Hubs
{
    public class ChatHub : Hub
    {
        //private string pushUrl = "rtmp://test-voicems-usea.channel.media.azure.net:1935/live/cd10160b1de44287bf459e0446cd7546/what";
        private string pushUrl = "rtmp://localhost:1935";
         
        //Push frame rate.
        private int frameRate = 15;
        private Pusher pusher;
        public async Task UploadStream(IAsyncEnumerable<string> stream, string name)
        //public async Task UploadStream(byte[] stream, string name)
        {
            Console.WriteLine("Video Stream incomming...");
            //pusher = new Pusher();
            //pusher.StartPush(pushUrl, 1280, 768, 15);
            // Convert to VideoPacket.

            await foreach (var packet in stream)
            {
                //_ = Task.Run(() => new StreamThread().CreateDnsThreadAsync(name, packet));
                // Wanna directly send to Azure inputUrl
                //byte[] bytes = Encoding.UTF8.GetBytes(packet);
                Console.WriteLine(packet);
                //GCHandle pinnedArray = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                //IntPtr pointer = pinnedArray.AddrOfPinnedObject();
                //// Do your stuff...
                //var videoPacket = new VideoPacket(pointer);
                //pusher.PushPacket(videoPacket);
                //pinnedArray.Free();
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
            Clients.User(userId).SendAsync("echo", name, message);
        }

        public void SendUsers(string name, IReadOnlyList<string> userIds, string message)
        {
            Clients.Users(userIds).SendAsync("echo", name, message);
        }
        public void Invite(string name, string username)
        {
            Console.WriteLine($"{name} send invitation to {username}");
            Clients.User(username).SendAsync("invite", name);
        }
        public void Typing(string name,string username, bool isTyping)
        {
            Clients.User(username).SendAsync("typing", name, isTyping);
        }
    }
}