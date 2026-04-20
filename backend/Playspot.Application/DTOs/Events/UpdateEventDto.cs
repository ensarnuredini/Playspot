namespace Playspot.Application.DTOs.Events;

public class UpdateEventDto
{
    public string Sport { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateTime { get; set; }
    public int TotalSpots { get; set; }
    public int? DurationMinutes { get; set; }
    public string SkillLevel { get; set; } = "All levels";
    public string Gender { get; set; } = "all";
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public bool RequiresApproval { get; set; } = false;
}
