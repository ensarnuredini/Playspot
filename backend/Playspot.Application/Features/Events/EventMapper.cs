using Playspot.Application.DTOs.Events;
using Playspot.Domain.Entities;

namespace Playspot.Application.Features.Events;

public static class EventMapper
{
    public static EventResponseDto MapToDto(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Sport = e.Sport,
        Location = e.Location,
        Description = e.Description,
        Date = e.Date,
        MaxParticipants = e.MaxParticipants,
        DurationMinutes = e.DurationMinutes,
        SkillLevel = e.SkillLevel,
        Gender = e.Gender,
        MinAge = e.MinAge,
        MaxAge = e.MaxAge,
        RequiresApproval = e.RequiresApproval,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        Status = e.Status,
        OrganizerId = e.OrganizerId,
        OrganizerName = e.Organizer?.Username ?? "Unknown",
        OrganizerImageUrl = e.Organizer?.ProfileImageUrl ?? string.Empty,
        ApprovedParticipantCount = e.JoinRequests.Count(jr => jr.Status == "Approved"),
        Participants = e.JoinRequests
            .Where(jr => jr.Status == "Approved")
            .Select(jr => new ParticipantDto
            {
                UserId = jr.UserId,
                Username = jr.User?.Username ?? "Unknown",
                IsHost = jr.UserId == e.OrganizerId,
                ProfileImageUrl = jr.User?.ProfileImageUrl ?? string.Empty
            }).ToList()
    };
}
