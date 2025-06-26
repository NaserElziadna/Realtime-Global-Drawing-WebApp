using Microsoft.EntityFrameworkCore;
using SignalRDrawingApp.Models;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public class DrawingSessionRepository : Repository<DrawingSession>, IDrawingSessionRepository
    {
        public DrawingSessionRepository(ApplicationDbContext context) 
            : base(context)
        {
        }
        
        private ApplicationDbContext AppDbContext => (ApplicationDbContext)Context;
        
        public async Task<DrawingSession?> GetWithStrokesAsync(int id)
        {
            return await AppDbContext.DrawingSessions
                .Include(s => s.Strokes)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        
        public async Task<DrawingSession> GetDefaultSessionAsync()
        {
            var session = await AppDbContext.DrawingSessions
                .Include(s => s.Strokes)
                .FirstOrDefaultAsync();
                
            if (session == null)
            {
                session = new DrawingSession 
                { 
                    SessionName = "Default Session",
                    UserName = "System",
                    CreatedAt = DateTime.UtcNow,
                    LastActivity = DateTime.UtcNow,
                    IsActive = true,
                    BackgroundColor = "#FFFFFF"
                };
                await AppDbContext.DrawingSessions.AddAsync(session);
                await AppDbContext.SaveChangesAsync();
            }
            
            return session;
        }
    }
} 