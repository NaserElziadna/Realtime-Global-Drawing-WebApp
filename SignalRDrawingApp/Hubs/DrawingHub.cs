using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Hubs
{
    public class DrawingHub : Hub
    {
        public async Task SendStroke(object stroke)
        {
            await Clients.Others.SendAsync("stroke", stroke);
        }

        public async Task SendStrokes(object strokes)
        {
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