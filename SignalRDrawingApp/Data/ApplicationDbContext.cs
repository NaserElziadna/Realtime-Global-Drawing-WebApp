using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SignalRDrawingApp.Models;

namespace SignalRDrawingApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
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
            
            // Configure DrawingSession
            modelBuilder.Entity<DrawingSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserName).HasMaxLength(100);
                entity.Property(e => e.BackgroundColor).HasDefaultValue("#FFFFFF");
                
                // Configure relationship with ApplicationUser
                entity.HasOne(e => e.User)
                      .WithMany(u => u.DrawingSessions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure DrawingStroke
            modelBuilder.Entity<DrawingStroke>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Points).IsRequired();
                entity.Property(e => e.Color).HasMaxLength(7).HasDefaultValue("#000000");
                entity.Property(e => e.Thickness).HasDefaultValue(2);
                entity.Property(e => e.UserName).HasMaxLength(100);
                
                // Configure relationship with ApplicationUser
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Configure relationship with DrawingSession
                entity.HasOne(e => e.Session)
                      .WithMany(s => s.Strokes)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ChatMessage
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                
                // Configure relationship with ApplicationUser
                entity.HasOne(e => e.User)
                      .WithMany(u => u.ChatMessages)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                // Configure relationship with DrawingSession
                entity.HasOne(e => e.Session)
                      .WithMany(s => s.ChatMessages)
                      .HasForeignKey(e => e.SessionId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.DisplayName).HasMaxLength(100);
                entity.Property(e => e.PreferredColor).HasMaxLength(50).HasDefaultValue("#007bff");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });
        }
    }
} 