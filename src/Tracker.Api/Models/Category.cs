
using System.ComponentModel.DataAnnotations;

namespace Tracker.Api.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Slug { get; set; }

    [MaxLength(10)]
    public string? Icon { get; set; } // Emoji or icon name

    [MaxLength(500)]
    public string? Description { get; set; }
}
