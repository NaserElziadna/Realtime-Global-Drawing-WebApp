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
        [StringLength(100)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Preferred Color")]
        public string PreferredColor { get; set; } = "#007bff";
    }
} 