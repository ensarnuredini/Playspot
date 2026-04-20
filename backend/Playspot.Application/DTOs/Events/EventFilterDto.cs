namespace Playspot.Application.DTOs.Events;

public class EventFilterDto
{
    public string? Sport { get; set; }
    public string? DateFilter { get; set; } // today | tomorrow | week | weekend
    public string? SkillLevel { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; } // date | distance | spots | popular
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool? HasOpenSpots { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
