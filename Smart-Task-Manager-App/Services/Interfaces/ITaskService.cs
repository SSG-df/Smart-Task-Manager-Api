using SmartTaskManager.DTOs;

namespace SmartTaskManager.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateAsync(TaskCreateDto dto, string updatedBy);
        Task<TaskDto?> GetByIdAsync(int id);
        Task<IEnumerable<TaskDto>> GetAllAsync();
        Task<TaskDto?> UpdateAsync(int id, TaskUpdateDto dto, string updatedBy);
        Task<bool> DeleteAsync(int id);
    }
}
