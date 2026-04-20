namespace Playspot.Domain.Entities;

public class EventRating
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}
