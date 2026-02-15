namespace Tracker.Api.Models;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
    public string? Genre { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public TrackingStatus Status { get; set; } = TrackingStatus.Upcoming;
    public string? Notes { get; set; }
    public string? CoverImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
}
