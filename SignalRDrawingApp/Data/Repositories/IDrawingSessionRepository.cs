using SignalRDrawingApp.Models;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public interface IDrawingSessionRepository : IRepository<DrawingSession>
    {
        Task<DrawingSession?> GetWithStrokesAsync(int id);
        Task<DrawingSession> GetDefaultSessionAsync();
    }
} 