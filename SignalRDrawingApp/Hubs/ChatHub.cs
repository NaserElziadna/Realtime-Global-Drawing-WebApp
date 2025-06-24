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

        public async Task JoinChat(string userName)
        {
            ConnectedUsers[Context.ConnectionId] = userName;
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values);
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            ConnectedUsers.TryRemove(Context.ConnectionId, out _);
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
