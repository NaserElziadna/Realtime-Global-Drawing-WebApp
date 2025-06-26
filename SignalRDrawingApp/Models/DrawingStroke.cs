using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SignalRDrawingApp.Models
{
    public class DrawingStroke
    {
        [Key]
        public int Id { get; set; }
        
        public string StrokeData { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int DrawingSessionId { get; set; }
        public DrawingSession? DrawingSession { get; set; }
        
        // Helper methods to convert between object and JSON string
        public static DrawingStroke FromObject(object strokeData, int sessionId)
        {
            return new DrawingStroke
            {
                StrokeData = JsonSerializer.Serialize(strokeData),
                DrawingSessionId = sessionId
            };
        }
        
        public object? ToObject()
        {
            if (string.IsNullOrEmpty(StrokeData))
                return null;
                
            return JsonSerializer.Deserialize<object>(StrokeData);
        }
    }
} 