namespace Playspot.Application.DTOs.Events;

public class EventResponseDto
{
    public int Id { get; set; }
    public string Sport { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateTime { get; set; }
    public int TotalSpots { get; set; }
    public int FilledSpots { get; set; }
    public int SpotsLeft => TotalSpots - FilledSpots;
    public string OrganiserUsername { get; set; } = string.Empty;
    public int OrganiserId { get; set; }
}