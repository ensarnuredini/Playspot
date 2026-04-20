namespace Playspot.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    // Navigation
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<JoinRequest> JoinRequests { get; set; } = new List<JoinRequest>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SavedEvent> SavedEvents { get; set; } = new List<SavedEvent>();
    public ICollection<EventRating> Ratings { get; set; } = new List<EventRating>();
}