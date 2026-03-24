namespace Playspot.Application.DTOs.Events;

public class CreateEventDto
{
    public string Sport { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime DateTime { get; set; }
    public int TotalSpots { get; set; }
}