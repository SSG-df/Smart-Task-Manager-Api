// DTOs/Tasks/TaskDto.cs
namespace SmartTaskManager.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = null!;
    public string Status { get; set; } = null!;
    public int AssignedUserId { get; set; }
    public string? AssignedUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RescheduledDate { get; set; }
    public string? LastUpdatedBy { get; set; }
    public DateTime LastUpdatedAt { get; set; }
}
