namespace Playspot.Domain.Entities;

public class EventReport
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int ReporterId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Event Event { get; set; } = null!;
    public User Reporter { get; set; } = null!;
}
