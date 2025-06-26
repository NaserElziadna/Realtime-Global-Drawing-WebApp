using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace SignalRDrawingApp.Models
{
    public class DrawingStroke
    {
        [Key]
        public int Id { get; set; }
        
        // Reference to Identity User
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        // Keep UserName for backward compatibility and guest users
        [StringLength(100)]
        public string? UserName { get; set; }
        
        [Required]
        public string Points { get; set; } = string.Empty;
        
        [StringLength(7)]
        public string Color { get; set; } = "#000000";
        
        public int Thickness { get; set; } = 2;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual DrawingSession Session { get; set; } = null!;
        
        // Helper methods to convert between object and JSON string
        public static DrawingStroke FromObject(object strokeData, int sessionId)
        {
            return new DrawingStroke
            {
                Points = JsonSerializer.Serialize(strokeData),
                SessionId = sessionId
            };
        }
        
        public object? ToObject()
        {
            if (string.IsNullOrEmpty(Points))
                return null;
                
            return JsonSerializer.Deserialize<object>(Points);
        }
    }
} 