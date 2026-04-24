using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;

namespace Playspot.Application.Features.Users.Queries;

public class GetUserJoinedEventsHandler : IRequestHandler<GetUserJoinedEventsQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _context;

    public GetUserJoinedEventsHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventResponseDto>> Handle(GetUserJoinedEventsQuery request, CancellationToken cancellationToken)
    {
        var joinedEventIds = await _context.JoinRequests
            .Where(jr => jr.UserId == request.UserId && jr.Status == "Accepted")
            .Select(jr => jr.EventId)
            .ToListAsync(cancellationToken);

        var events = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests)
                .ThenInclude(jr => jr.User)
            .Where(e => joinedEventIds.Contains(e.Id))
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);

        return events.Select(e => new EventResponseDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Sport = e.Sport,
            Date = e.Date,
            Location = e.Location,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            SkillLevel = e.SkillLevel,
            MaxParticipants = e.MaxParticipants,
            ApprovedParticipantCount = e.JoinRequests.Count(jr => jr.Status == "Accepted"),
            OrganizerId = e.OrganizerId,
            OrganizerName = e.Organizer?.Username ?? "Unknown Organizer",
            OrganizerImageUrl = e.Organizer?.ProfileImageUrl ?? string.Empty
        }).ToList();
    }
}
