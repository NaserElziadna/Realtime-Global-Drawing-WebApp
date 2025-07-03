using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SignalRDrawingApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? PreferredColor { get; set; } = "#007bff";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties for related entities
        public virtual ICollection<DrawingSession> DrawingSessions { get; set; } = new List<DrawingSession>();
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
} 