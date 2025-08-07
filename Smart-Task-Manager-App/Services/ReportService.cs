using SmartTaskManager.Data;
using SmartTaskManager.DTOs;
using SmartTaskManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace SmartTaskManager.Services
{
    public class ReportService : IReportService
    {
        private readonly SmartTaskManagerDbContext _context;

        public ReportService(SmartTaskManagerDbContext context)
        {
            _context = context;
        }

        public async Task<List<TaskCompletionReportDto>> GetTaskCompletionReportAsync()
        {
            var lastWeek = DateTime.UtcNow.AddDays(-7);

            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.Status == SmartTaskManager.Models.TaskStatus.Completed && t.CompletedAt >= lastWeek)
                .ToListAsync();

            if (!tasks.Any())
                throw new InvalidOperationException("No completed tasks found in the last 7 days.");

            var grouped = tasks
                .GroupBy(t => t.AssignedUser?.Username)
                .Where(g => g.Key != null)
                .Select(g => new TaskCompletionReportDto
                {
                    UserName = g.Key!,
                    TasksCompleted = g.Count()
                })
                .ToList();

            if (grouped.Count == 0)
                throw new InvalidOperationException("No valid users found for task completion report.");

            return grouped;
        }

        public async Task<List<AverageCompletionTimeDto>> GetAverageCompletionTimeReportAsync()
        {
            var tasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.Status == Models.TaskStatus.Completed && t.CompletedAt != null)
                .ToListAsync();

            if (!tasks.Any())
                throw new InvalidOperationException("No completed tasks with completion dates found.");

            var grouped = tasks
                .GroupBy(t => t.AssignedUser?.Username)
                .Where(g => g.Key != null)
                .Select(g => new AverageCompletionTimeDto
                {
                    UserName = g.Key!,
                    AverageHours = g.Average(t =>
                        (t.CompletedAt!.Value - t.CreatedAt).TotalHours)
                })
                .ToList();

            if (grouped.Count == 0)
                throw new InvalidOperationException("No valid users found for average completion time report.");

            return grouped;
        }
    }
}