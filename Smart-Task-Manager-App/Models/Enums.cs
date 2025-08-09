namespace SmartTaskManager.Models
{
    public enum UserRole
    {
        RegularUser,
        Admin
    }

    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Critical = 3
    }

    public enum TaskStatus
    {
        New = 0,
        InProgress = 1,
        Completed = 2,
        Overdue = 3,
        Cancelled = 4
    }

    public enum ReportPeriod
    {
        Last7Days = 0,
        LastMonth = 1,
        Last90Days = 2,
        Custom = 3
    }
}