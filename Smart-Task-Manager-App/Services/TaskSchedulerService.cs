using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Data;
using SmartTaskManager.Models;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace SmartTaskManager.Services
{
    public class TaskSchedulerService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TaskSchedulerService> _logger;
        private const int RescheduleDays = 1;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public TaskSchedulerService(
            IServiceScopeFactory scopeFactory, 
            ILogger<TaskSchedulerService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Scheduler Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOverdueTasksAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing overdue tasks");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Task Scheduler Service stopped");
        }

        private async Task ProcessOverdueTasksAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SmartTaskManagerDbContext>();

            var overdueTasks = await context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.DueDate < DateTime.UtcNow && 
                           t.Status != Models.TaskStatus.Completed)
                .ToListAsync(stoppingToken);

            foreach (var task in overdueTasks)
            {
                var originalDueDate = task.DueDate;
                task.DueDate = originalDueDate.AddDays(RescheduleDays);
                task.Status = Models.TaskStatus.Overdue;
                task.RescheduledDate = DateTime.UtcNow;
                
                _logger.LogInformation(
                    "Task '{Title}' (ID: {TaskId}) rescheduled from {OldDate} to {NewDate}",
                    task.Title,
                    task.Id,
                    originalDueDate,
                    task.DueDate);

                await SendMockEmailNotificationAsync(task, originalDueDate);
            }

            if (overdueTasks.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
            }
        }

        private async Task SendMockEmailNotificationAsync(Models.Task task, DateTime originalDueDate)
        {
            try
            {
                var userEmail = task.AssignedUser?.Email ?? "unknown@email.com";
                var userName = task.AssignedUser?.Username ?? "Unknown User";
                
                _logger.LogInformation(
                    "📧 MOCK EMAIL SENT to {UserEmail} ({UserName}): " +
                    "Your task '{TaskTitle}' (ID: {TaskId}) was overdue on {OriginalDueDate} " +
                    "and has been automatically rescheduled to {NewDueDate}. " +
                    "Please complete it as soon as possible.",
                    userEmail,
                    userName,
                    task.Title,
                    task.Id,
                    originalDueDate.ToString("yyyy-MM-dd HH:mm"),
                    task.DueDate.ToString("yyyy-MM-dd HH:mm"));

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to send mock email notification for task {TaskId} to user {UserId}",
                    task.Id, 
                    task.AssignedUserId);
            }
        }
    }
}