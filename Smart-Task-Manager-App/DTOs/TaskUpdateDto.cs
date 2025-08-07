namespace SmartTaskManager.DTOs;

public class TaskUpdateDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public DateTime? RescheduledDate { get; set; }
}
