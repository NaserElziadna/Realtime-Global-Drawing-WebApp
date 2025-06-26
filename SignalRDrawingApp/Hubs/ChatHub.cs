using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;

namespace SignalRDrawingApp.Hubs
{
    public class ChatHub : Hub
    {
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();
        private static List<(string User, string Message)> MessageHistory = new List<(string, string)>();
        private const int MAX_HISTORY = 100; // Limit history to prevent memory issues

        public async Task JoinChat(string userName)
        {
            ConnectedUsers[Context.ConnectionId] = userName;
            
            // Send message history to the joining user
            foreach (var (user, message) in MessageHistory)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", user, message);
            }
            
            await UpdateUserList();
            await Clients.All.SendAsync("updateUserCount", ConnectedUsers.Count);
            
            // Announce new user joined
            await Clients.Others.SendAsync("ReceiveMessage", "System", $"{userName} joined the chat");
        }

        public async Task SendMessage(string user, string message)
        {
            // Add to history
            MessageHistory.Add((user, message));
            
            // Trim history if it gets too large
            if (MessageHistory.Count > MAX_HISTORY)
            {
                MessageHistory.RemoveAt(0);
            }
            
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out string userName))
            {
                await Clients.Others.SendAsync("ReceiveMessage", "System", $"{userName} left the chat");
            }
            
            await UpdateUserList();
            await Clients.All.SendAsync("updateUserCount", ConnectedUsers.Count);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateUserList()
        {
            var users = ConnectedUsers.Values;
            await Clients.All.SendAsync("UpdateUserList", users);
        }
    }
}
