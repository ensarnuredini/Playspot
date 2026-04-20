namespace Playspot.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MaxParticipants { get; set; }
    public int? DurationMinutes { get; set; }
    public string SkillLevel { get; set; } = "All levels"; // All levels | Beginner | Intermediate | Advanced
    public string Gender { get; set; } = "all"; // all | male | female
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public bool RequiresApproval { get; set; } = false;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Status { get; set; } = "Active"; // Active | Cancelled | Completed
    public int OrganizerId { get; set; }

    // Navigation properties
    public User Organizer { get; set; } = null!;
    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
    public ICollection<EventRating> Ratings { get; set; } = new List<EventRating>();
    public ICollection<EventReport> Reports { get; set; } = new List<EventReport>();
}