using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;

namespace Playspot.Application.Features.Events.Queries;

// ── Get Event by ID ──
public record GetEventByIdQuery(int Id) : IRequest<EventResponseDto?>;

public class GetEventByIdHandler : IRequestHandler<GetEventByIdQuery, EventResponseDto?>
{
    private readonly IAppDbContext _db;
    public GetEventByIdHandler(IAppDbContext db) => _db = db;

    public async Task<EventResponseDto?> Handle(GetEventByIdQuery request, CancellationToken ct)
    {
        var e = await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);

        return e == null ? null : EventMapper.MapToDto(e);
    }
}

// ── Get Filtered Events ──
public record GetFilteredEventsQuery(EventFilterDto Filters) : IRequest<List<EventResponseDto>>;

public class GetFilteredEventsHandler : IRequestHandler<GetFilteredEventsQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetFilteredEventsHandler(IAppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> Handle(GetFilteredEventsQuery request, CancellationToken ct)
    {
        var filters = request.Filters;
        var query = _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.Status == "Active")
            .AsQueryable();

        if (!string.IsNullOrEmpty(filters.Sport))
            query = query.Where(e => e.Sport.ToLower() == filters.Sport.ToLower());

        if (!string.IsNullOrEmpty(filters.SkillLevel))
            query = query.Where(e => e.SkillLevel.ToLower() == filters.SkillLevel.ToLower());

        if (!string.IsNullOrEmpty(filters.DateFilter))
        {
            var today = DateTime.UtcNow.Date;
            query = filters.DateFilter.ToLower() switch
            {
                "today" => query.Where(e => e.Date.Date == today),
                "tomorrow" => query.Where(e => e.Date.Date == today.AddDays(1)),
                "week" => query.Where(e => e.Date.Date >= today && e.Date.Date <= today.AddDays(7)),
                "weekend" => query.Where(e => e.Date.DayOfWeek == DayOfWeek.Saturday || e.Date.DayOfWeek == DayOfWeek.Sunday),
                _ => query
            };
        }

        if (!string.IsNullOrEmpty(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(search) ||
                e.Location.ToLower().Contains(search) ||
                e.Sport.ToLower().Contains(search));
        }

        if (filters.HasOpenSpots == true)
            query = query.Where(e => e.JoinRequests.Count(jr => jr.Status == "Approved") < e.MaxParticipants);

        query = filters.SortBy?.ToLower() switch
        {
            "date" => query.OrderBy(e => e.Date),
            "spots" => query.OrderByDescending(e => e.MaxParticipants - e.JoinRequests.Count(jr => jr.Status == "Approved")),
            "popular" => query.OrderByDescending(e => e.JoinRequests.Count(jr => jr.Status == "Approved")),
            _ => query.OrderBy(e => e.Date)
        };

        return await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(e => EventMapper.MapToDto(e))
            .ToListAsync(ct);
    }
}

// ── Get My Hosting ──
public record GetMyHostingQuery(int UserId) : IRequest<List<EventResponseDto>>;

public class GetMyHostingHandler : IRequestHandler<GetMyHostingQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetMyHostingHandler(IAppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> Handle(GetMyHostingQuery request, CancellationToken ct)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.OrganizerId == request.UserId && e.Status == "Active")
            .OrderBy(e => e.Date)
            .Select(e => EventMapper.MapToDto(e))
            .ToListAsync(ct);
    }
}

// ── Get My Joined ──
public record GetMyJoinedQuery(int UserId) : IRequest<List<EventResponseDto>>;

public class GetMyJoinedHandler : IRequestHandler<GetMyJoinedQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetMyJoinedHandler(IAppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> Handle(GetMyJoinedQuery request, CancellationToken ct)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.JoinRequests.Any(jr => jr.UserId == request.UserId && jr.Status == "Approved")
                        && e.Status == "Active" && e.Date >= DateTime.UtcNow)
            .OrderBy(e => e.Date)
            .Select(e => EventMapper.MapToDto(e))
            .ToListAsync(ct);
    }
}

// ── Get My Past ──
public record GetMyPastQuery(int UserId) : IRequest<List<EventResponseDto>>;

public class GetMyPastHandler : IRequestHandler<GetMyPastQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetMyPastHandler(IAppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> Handle(GetMyPastQuery request, CancellationToken ct)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => (e.OrganizerId == request.UserId ||
                         e.JoinRequests.Any(jr => jr.UserId == request.UserId && jr.Status == "Approved"))
                        && (e.Date < DateTime.UtcNow || e.Status == "Completed"))
            .OrderByDescending(e => e.Date)
            .Select(e => EventMapper.MapToDto(e))
            .ToListAsync(ct);
    }
}

// ── Get Similar Events ──
public record GetSimilarEventsQuery(int EventId) : IRequest<List<EventResponseDto>>;

public class GetSimilarEventsHandler : IRequestHandler<GetSimilarEventsQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetSimilarEventsHandler(IAppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> Handle(GetSimilarEventsQuery request, CancellationToken ct)
    {
        var ev = await _db.Events.FindAsync([request.EventId], ct);
        if (ev == null) return [];

        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.Id != request.EventId && e.Sport == ev.Sport
                        && e.Status == "Active" && e.Date >= DateTime.UtcNow)
            .OrderBy(e => e.Date)
            .Take(5)
            .Select(e => EventMapper.MapToDto(e))
            .ToListAsync(ct);
    }
}
