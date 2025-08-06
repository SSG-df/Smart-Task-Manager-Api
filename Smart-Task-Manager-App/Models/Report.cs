using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTaskManager.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
        public ReportPeriod Period { get; set; } = ReportPeriod.Last7Days;
        public int TasksCompleted { get; set; }
        public double AverageCompletionTimeHours { get; set; }
        public int OverdueTasksCount { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}