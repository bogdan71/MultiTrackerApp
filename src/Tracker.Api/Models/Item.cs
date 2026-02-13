
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tracker.Api.Models;

public class Item
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Active";

    public string? Properties { get; set; } // JSON blob for flexible properties

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
