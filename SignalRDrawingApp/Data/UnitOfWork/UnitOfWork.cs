using SignalRDrawingApp.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        
        public IDrawingSessionRepository DrawingSessions { get; private set; }
        public IDrawingStrokeRepository DrawingStrokes { get; private set; }
        
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            DrawingSessions = new DrawingSessionRepository(context);
            DrawingStrokes = new DrawingStrokeRepository(context);
        }
        
        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
} 