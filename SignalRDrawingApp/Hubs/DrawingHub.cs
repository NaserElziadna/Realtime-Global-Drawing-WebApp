using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SignalRDrawingApp.Hubs
{
    public class DrawingHub : Hub
    {
        public static List<object> BoardHistory { get; set; } = new List<object>();

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await SendBoard(BoardHistory);
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
    }
} 