using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace SignalRDrawingApp.Hubs
{
    public class DrawingHub : Hub
    {
        public static List<object> BoardHistory { get; set; } = new List<object>();
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task SendStroke(object stroke)
        {
            BoardHistory.Add(stroke);
            await Clients.Others.SendAsync("stroke", stroke);
        }

        public async Task SendStrokes(object strokes)
        {
            if (strokes is IEnumerable<object> strokesList)
            {
                BoardHistory.AddRange(strokesList);
            }
            await Clients.Others.SendAsync("strokes", strokes);
        }

        public async Task SendDelete(object data)
        {
            await Clients.Others.SendAsync("delete", data);
        }

        public async Task SendCursorMove(object data)
        {
            await Clients.Others.SendAsync("cursormove", data);
        }

        public async Task SendBackgroundColour(object data)
        {
            await Clients.Others.SendAsync("backgroundcolour", data);
        }

        public async Task SendBoard(object board)
        {
            await Clients.Caller.SendAsync("board", board);
        }

        public async Task JoinRoom(string userName)
        {
            ConnectedUsers.TryAdd(Context.ConnectionId, userName);
            await Clients.Others.SendAsync("userJoined", userName);
            await Clients.Caller.SendAsync("board", BoardHistory);
            await UpdateUserCount();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out string userName))
            {
                await Clients.Others.SendAsync("userLeft", userName);
                await UpdateUserCount();
            }
            await base.OnDisconnectedAsync(exception);
        }

        private async Task UpdateUserCount()
        {
            await Clients.All.SendAsync("updateUserCount", ConnectedUsers.Count);
        }
    }
}