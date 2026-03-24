namespace Playspot.Domain.Entities;

public enum JoinStatus { Pending, Approved, Declined }

public class JoinRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public JoinStatus Status { get; set; } = JoinStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}