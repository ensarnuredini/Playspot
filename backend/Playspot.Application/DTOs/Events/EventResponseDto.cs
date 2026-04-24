namespace Playspot.Application.DTOs.Events;

public class EventResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Sport { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MaxParticipants { get; set; }
    public int? DurationMinutes { get; set; }
    public string SkillLevel { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public bool RequiresApproval { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Status { get; set; } = string.Empty;
    public int OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public string OrganizerImageUrl { get; set; } = string.Empty;
    public int ApprovedParticipantCount { get; set; }
    public List<ParticipantDto> Participants { get; set; } = new();
}

public class ParticipantDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public string ProfileImageUrl { get; set; } = string.Empty;
}