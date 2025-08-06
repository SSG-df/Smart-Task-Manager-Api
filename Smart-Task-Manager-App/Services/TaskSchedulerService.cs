namespace Smart_Task_Manager_App.Services
{
    public class TaskSchedulerService(IServiceScopeFactory scopeFactory, ILogger<TaskSchedulerService> logger) : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<TaskSchedulerService> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DbContext>();

                var overdueTasks = await context.Tasks
                    .Where(t => t.DueDate < DateTime.UtcNow && t.Status != "Completed")
                    .ToListAsync();

                foreach (var task in overdueTasks)
                {
                    var oldDate = task.DueDate;
                    task.DueDate = oldDate.AddDays(1);
                    _logger.LogInformation($"[Email] Task '{task.Title}' was rescheduled from {oldDate} to {task.DueDate} for user {task.AssignedUserId}");
                }

                await context.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}