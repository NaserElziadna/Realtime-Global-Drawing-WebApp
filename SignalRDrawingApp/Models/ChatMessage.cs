using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalRDrawingApp.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        
        // Reference to Identity User
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        
        // Keep UserName for backward compatibility and guest users
        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public int? SessionId { get; set; }
        [ForeignKey("SessionId")]
        public virtual DrawingSession? Session { get; set; }
    }
} 