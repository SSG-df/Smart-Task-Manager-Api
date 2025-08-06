using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartTaskManager.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string? Username { get; set; }
        [Required]
        [JsonIgnore]
        public string? PasswordHash { get; set; }
        [Required]
        public UserRole Role { get; set; } = UserRole.RegularUser;
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        [JsonIgnore]
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}