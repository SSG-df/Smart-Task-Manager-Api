using SmartTaskManager.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartTaskManager.DTOs.Auth
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.RegularUser;
    }
}
