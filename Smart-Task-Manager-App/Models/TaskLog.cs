using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskManager.Models
{
    public class TaskLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        public Task? Task { get; set; }
        public DateTime OldDueDate { get; set; }
        public DateTime NewDueDate { get; set; }
        public DateTime RescheduledAt { get; set; } = DateTime.UtcNow;
        public string? Reason { get; set; }
    }
}