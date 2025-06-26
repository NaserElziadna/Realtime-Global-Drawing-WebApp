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
        public DbSet<ChatMessage> ChatMessages { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure relationships for DrawingStroke
            modelBuilder.Entity<DrawingStroke>()
                .HasOne(s => s.DrawingSession)
                .WithMany(s => s.Strokes)
                .HasForeignKey(s => s.DrawingSessionId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure relationships for ChatMessage
            modelBuilder.Entity<ChatMessage>()
                .HasOne(c => c.DrawingSession)
                .WithMany(s => s.ChatMessages)
                .HasForeignKey(c => c.DrawingSessionId)
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