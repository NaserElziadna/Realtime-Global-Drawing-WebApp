using Microsoft.AspNetCore.SignalR;
using SignalRDrawingApp.Data.UnitOfWork;
using SignalRDrawingApp.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Hubs
{
    public class DrawingHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();
        private static int DefaultSessionId = 1;

        public DrawingHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await SendBoard();
        }

        public async Task SendStroke(object stroke)
        {
            // Save stroke to database
            await _unitOfWork.DrawingStrokes.AddStrokeFromObjectAsync(stroke, DefaultSessionId);
            await _unitOfWork.CompleteAsync();
            
            // Send to other clients
            await Clients.Others.SendAsync("stroke", stroke);
        }

        public async Task SendStrokes(object strokes)
        {
            if (strokes is IEnumerable<object> strokesList)
            {
                // Save strokes to database
                foreach (var stroke in strokesList)
                {
                    await _unitOfWork.DrawingStrokes.AddStrokeFromObjectAsync(stroke, DefaultSessionId);
                }
                await _unitOfWork.CompleteAsync();
            }
            
            // Send to other clients
            await Clients.Others.SendAsync("strokes", strokes);
        }

        public async Task SendDelete(object data)
        {
            // For now, we don't handle deletion in the database
            // This would require additional logic to track which strokes to delete
            
            await Clients.Others.SendAsync("delete", data);
        }

        public async Task SendCursorMove(object data)
        {
            // Cursor movements are not stored in the database
            await Clients.Others.SendAsync("cursormove", data);
        }

        public async Task SendBackgroundColour(object data)
        {
            // Update the background color in the database
            var session = await _unitOfWork.DrawingSessions.GetByIdAsync(DefaultSessionId);
            if (session != null && data != null)
            {
                // Extract the color from the data object
                // Assuming data has a property called "color"
                var colorProperty = data.GetType().GetProperty("color");
                if (colorProperty != null)
                {
                    string? color = colorProperty.GetValue(data)?.ToString();
                    if (!string.IsNullOrEmpty(color))
                    {
                        session.BackgroundColor = color;
                        session.LastModifiedAt = DateTime.UtcNow;
                        _unitOfWork.DrawingSessions.Update(session);
                        await _unitOfWork.CompleteAsync();
                    }
                }
            }
            
            await Clients.Others.SendAsync("backgroundcolour", data);
        }

        public async Task SendBoard()
        {
            // Get strokes from database
            var session = await _unitOfWork.DrawingSessions.GetDefaultSessionAsync();
            var strokes = await _unitOfWork.DrawingStrokes.GetStrokeObjectsBySessionIdAsync(session.Id);
            
            await Clients.Caller.SendAsync("board", strokes);
        }

        public async Task JoinRoom(string userName)
        {
            ConnectedUsers[Context.ConnectionId] = userName;
            await Clients.All.SendAsync("updateUserCount", ConnectedUsers.Count);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out string userName))
            {
                await Clients.Others.SendAsync("userLeft", userName);
                await Clients.All.SendAsync("updateUserCount", ConnectedUsers.Count);
                await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Static methods for admin monitoring
        public static int GetConnectedUsersCount()
        {
            return ConnectedUsers.Count;
        }

        public static List<string> GetConnectedUsersList()
        {
            return ConnectedUsers.Values.ToList();
        }
    }
}