namespace Tracker.Api.Models;

public class TodoItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? DueDate { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public bool IsCompleted { get; set; }
    public string? Category { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
}
