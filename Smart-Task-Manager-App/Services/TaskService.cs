using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Data;
using SmartTaskManager.DTOs;
using SmartTaskManager.Interfaces;
using SmartTaskManager.Models;

namespace SmartTaskManager.Services
{
    public class TaskService : ITaskService
    {
        private readonly SmartTaskManagerDbContext _context;

        public TaskService(SmartTaskManagerDbContext context)
        {
            _context = context;
        }

        public async Task<TaskDto> CreateAsync(TaskCreateDto dto, string updatedBy)
        {
            bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId);
            if (!userExists)
            {
                throw new ArgumentException("User not found");
            }

            if (dto.DueDate <= DateTime.UtcNow)
            {
                throw new ArgumentException("Due date must be in future");
            }

            var task = new Models.Task
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = Enum.Parse<TaskPriority>(dto.Priority, true),
                Status = Models.TaskStatus.New,
                AssignedUserId = dto.AssignedUserId,
                LastUpdatedBy = updatedBy,
                CreatedAt = DateTime.UtcNow,
                LastUpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return await MapToDto(task.Id);
        }

        public async Task<TaskDto?> GetByIdAsync(int id)
        {
            return await MapToDto(id);
        }

        public async Task<IEnumerable<TaskDto>> GetAllAsync()
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    AssignedUserId = t.AssignedUserId,
                    AssignedUsername = t.AssignedUser.Username,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    RescheduledDate = t.RescheduledDate,
                    LastUpdatedBy = t.LastUpdatedBy,
                    LastUpdatedAt = t.LastUpdatedAt
                })
                .ToListAsync();
        }

        public async Task<TaskDto?> UpdateAsync(int id, TaskUpdateDto dto, string updatedBy)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return null;
            }

            if (dto.Title != null)
            {
                task.Title = dto.Title;
            }

            if (dto.Description != null)
            {
                task.Description = dto.Description;
            }

            if (dto.DueDate.HasValue)
            {
                task.DueDate = dto.DueDate.Value;
            }

            if (dto.Priority != null)
            {
                task.Priority = Enum.Parse<TaskPriority>(dto.Priority, true);
            }

            if (dto.Status != null)
            {
                task.Status = Enum.Parse<Models.TaskStatus>(dto.Status, true);
            }

            if (dto.RescheduledDate.HasValue)
            {
                task.RescheduledDate = dto.RescheduledDate.Value;
            }

            task.LastUpdatedBy = updatedBy;
            task.LastUpdatedAt = DateTime.UtcNow;

            if (task.Status == Models.TaskStatus.Completed && task.CompletedAt == null)
            {
                task.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return await MapToDto(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return false;
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<TaskDto?> MapToDto(int id)
        {
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.Id == id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    Priority = t.Priority.ToString(),
                    Status = t.Status.ToString(),
                    AssignedUserId = t.AssignedUserId,
                    AssignedUsername = t.AssignedUser.Username,
                    CreatedAt = t.CreatedAt,
                    CompletedAt = t.CompletedAt,
                    RescheduledDate = t.RescheduledDate,
                    LastUpdatedBy = t.LastUpdatedBy,
                    LastUpdatedAt = t.LastUpdatedAt
                })
                .FirstOrDefaultAsync();
        }
    }
}