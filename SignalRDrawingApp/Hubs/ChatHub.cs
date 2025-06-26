using Microsoft.AspNetCore.SignalR;
using SignalRDrawingApp.Data.UnitOfWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();
        private static int DefaultSessionId = 1;
        private const int MAX_HISTORY = 100; // Limit history to prevent memory issues
        
        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task JoinChat(string userName)
        {
            ConnectedUsers[Context.ConnectionId] = userName;
            
            // Send message history from database to the joining user
            var messages = await _unitOfWork.ChatMessages.GetBySessionIdAsync(DefaultSessionId, MAX_HISTORY);
            foreach (var message in messages)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", message.Username, message.Message);
            }
            
            await UpdateUserList();
            await Clients.All.SendAsync("updateUserCount", ConnectedUsers.Count);
            
            // Announce new user joined and save to database
            string systemMessage = $"{userName} joined the chat";
            await _unitOfWork.ChatMessages.AddMessageAsync("System", systemMessage, DefaultSessionId);
            await _unitOfWork.CompleteAsync();
            
            await Clients.Others.SendAsync("ReceiveMessage", "System", systemMessage);
        }

        public async Task SendMessage(string user, string message)
        {
            // Add to database
            await _unitOfWork.ChatMessages.AddMessageAsync(user, message, DefaultSessionId);
            await _unitOfWork.CompleteAsync();
            
            // Send to all clients
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out string userName))
            {
                // Save disconnect message to database
                string systemMessage = $"{userName} left the chat";
                await _unitOfWork.ChatMessages.AddMessageAsync("System", systemMessage, DefaultSessionId);
                await _unitOfWork.CompleteAsync();
                
                await Clients.Others.SendAsync("ReceiveMessage", "System", systemMessage);
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
