using System;
using System.ComponentModel.DataAnnotations;

namespace SignalRDrawingApp.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        public string Message { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int DrawingSessionId { get; set; }
        public DrawingSession? DrawingSession { get; set; }
    }
} 