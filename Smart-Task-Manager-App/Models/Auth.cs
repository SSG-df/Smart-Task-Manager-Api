using System.ComponentModel.DataAnnotations;
using SmartTaskManager.Models;


namespace SmartTaskManager.Models
{
    public class AuthRequest
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }

    public class AuthResponse
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}