using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;

namespace Playspot.Application.Features.Users.Queries;

public class GetUserEventsHandler : IRequestHandler<GetUserEventsQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _context;

    public GetUserEventsHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventResponseDto>> Handle(GetUserEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests)
                .ThenInclude(jr => jr.User)
            .Where(e => e.OrganizerId == request.UserId)
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
