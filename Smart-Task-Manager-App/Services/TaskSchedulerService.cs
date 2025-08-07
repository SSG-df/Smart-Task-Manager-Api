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
                .Where(t => t.DueDate < DateTime.UtcNow && 
                           t.Status != Models.TaskStatus.Completed)
                .ToListAsync(stoppingToken);

            foreach (var task in overdueTasks)
            {
                var originalDueDate = task.DueDate;
                task.DueDate = originalDueDate.AddDays(RescheduleDays);
                task.Status = Models.TaskStatus.Overdue;
                
                _logger.LogInformation(
                    "Task '{Title}' (ID: {TaskId}) rescheduled from {OldDate} to {NewDate}",
                    task.Title,
                    task.Id,
                    originalDueDate,
                    task.DueDate);
            }

            if (overdueTasks.Any())
            {
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}