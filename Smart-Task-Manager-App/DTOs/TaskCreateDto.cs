namespace SmartTaskManager.DTOs;

public class TaskCreateDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public string Priority { get; set; } = "Medium";
    public int AssignedUserId { get; set; }
}

