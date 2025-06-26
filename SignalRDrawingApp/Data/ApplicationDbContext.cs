using Microsoft.EntityFrameworkCore;
using SignalRDrawingApp.Models;

namespace SignalRDrawingApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<DrawingSession> DrawingSessions { get; set; }
        public DbSet<DrawingStroke> DrawingStrokes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships
            modelBuilder.Entity<DrawingStroke>()
                .HasOne(s => s.DrawingSession)
                .WithMany(s => s.Strokes)
                .HasForeignKey(s => s.DrawingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Create a default drawing session
            modelBuilder.Entity<DrawingSession>().HasData(
                new DrawingSession
                {
                    Id = 1,
                    Name = "Default Session",
                    CreatedAt = System.DateTime.UtcNow
                }
            );
        }
    }
} 