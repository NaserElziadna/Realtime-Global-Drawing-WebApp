using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SignalRDrawingApp.Models
{
    public class DrawingSession
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; } = "Default Session";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastModifiedAt { get; set; }
        
        public string BackgroundColor { get; set; } = "#FFFFFF";
        
        // Navigation property for strokes
        public ICollection<DrawingStroke> Strokes { get; set; } = new List<DrawingStroke>();
    }
} 