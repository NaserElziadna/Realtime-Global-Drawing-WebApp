using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalRDrawingApp.Models
{
    public class DrawingSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string SessionName { get; set; } = string.Empty;
        
        // Reference to Identity User
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        // Keep UserName for backward compatibility and guest users
        [StringLength(100)]
        public string? UserName { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        public string BackgroundColor { get; set; } = "#FFFFFF";
        
        // Navigation property for strokes
        public virtual ICollection<DrawingStroke> Strokes { get; set; } = new List<DrawingStroke>();
        
        // Navigation property for chat messages
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
} 