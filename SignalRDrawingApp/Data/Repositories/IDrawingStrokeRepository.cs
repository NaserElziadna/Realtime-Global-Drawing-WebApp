using SignalRDrawingApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public interface IDrawingStrokeRepository : IRepository<DrawingStroke>
    {
        Task<IEnumerable<DrawingStroke>> GetBySessionIdAsync(int sessionId);
        Task<DrawingStroke> AddStrokeFromObjectAsync(object strokeData, int sessionId);
        Task<IEnumerable<object>> GetStrokeObjectsBySessionIdAsync(int sessionId);
    }
} 