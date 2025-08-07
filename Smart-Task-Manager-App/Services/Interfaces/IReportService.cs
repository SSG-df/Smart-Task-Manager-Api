using SmartTaskManager.DTOs;

namespace SmartTaskManager.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<TaskCompletionReportDto>> GetTaskCompletionReportAsync();
        Task<List<AverageCompletionTimeDto>> GetAverageCompletionTimeReportAsync();
    }
}