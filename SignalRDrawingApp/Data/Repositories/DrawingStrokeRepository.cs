using Microsoft.EntityFrameworkCore;
using SignalRDrawingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public class DrawingStrokeRepository : Repository<DrawingStroke>, IDrawingStrokeRepository
    {
        public DrawingStrokeRepository(ApplicationDbContext context) 
            : base(context)
        {
        }
        
        private ApplicationDbContext AppDbContext => (ApplicationDbContext)Context;
        
        public async Task<IEnumerable<DrawingStroke>> GetBySessionIdAsync(int sessionId)
        {
            return await AppDbContext.DrawingStrokes
                .Where(s => s.DrawingSessionId == sessionId)
                .ToListAsync();
        }
        
        public async Task<DrawingStroke> AddStrokeFromObjectAsync(object strokeData, int sessionId)
        {
            var stroke = DrawingStroke.FromObject(strokeData, sessionId);
            await AddAsync(stroke);
            return stroke;
        }
        
        public async Task<IEnumerable<object>> GetStrokeObjectsBySessionIdAsync(int sessionId)
        {
            var strokes = await GetBySessionIdAsync(sessionId);
            return strokes
                .Select(s => s.ToObject())
                .Where(o => o != null)
                .Cast<object>()
                .ToList();
        }
    }
} 