using System.ComponentModel.DataAnnotations;

namespace SignalRDrawingApp.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Nickname")]
        public string Nickname { get; set; } = string.Empty;

        [StringLength(7)]
        [Display(Name = "Preferred Color")]
        public string PreferredColor { get; set; } = "#007bff";
    }
} 