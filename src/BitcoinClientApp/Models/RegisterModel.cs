
using System.ComponentModel.DataAnnotations;

namespace BitcoinClientApp.Models
{
    public class RegisterModel
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? DisplayName { get; set; }
    }
}