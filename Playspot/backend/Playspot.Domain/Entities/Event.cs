namespace Playspot.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Sport { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateTime { get; set; }
    public int TotalSpots { get; set; }
    public int FilledSpots { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int OrganiserId { get; set; }
    public User Organiser { get; set; } = null!;
    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
}