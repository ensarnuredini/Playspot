using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.DTOs.Ratings;
using Playspot.Application.DTOs.Reports;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;

namespace Playspot.Application.Features.EventActions;

// ── Save Event ──
public record SaveEventCommand(int EventId, int UserId) : IRequest<bool>;

public class SaveEventHandler : IRequestHandler<SaveEventCommand, bool>
{
    private readonly IAppDbContext _db;
    public SaveEventHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(SaveEventCommand request, CancellationToken ct)
    {
        if (await _db.SavedEvents.AnyAsync(s => s.EventId == request.EventId && s.UserId == request.UserId, ct))
            return false;

        _db.SavedEvents.Add(new SavedEvent { EventId = request.EventId, UserId = request.UserId });
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

// ── Unsave Event ──
public record UnsaveEventCommand(int EventId, int UserId) : IRequest<bool>;

public class UnsaveEventHandler : IRequestHandler<UnsaveEventCommand, bool>
{
    private readonly IAppDbContext _db;
    public UnsaveEventHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(UnsaveEventCommand request, CancellationToken ct)
    {
        var saved = await _db.SavedEvents
            .FirstOrDefaultAsync(s => s.EventId == request.EventId && s.UserId == request.UserId, ct);
        if (saved == null) return false;

        _db.SavedEvents.Remove(saved);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

// ── Get Saved Events ──
public record GetSavedEventsQuery(int UserId) : IRequest<List<EventResponseDto>>;

public class GetSavedEventsHandler : IRequestHandler<GetSavedEventsQuery, List<EventResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetSavedEventsHandler(IAppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> Handle(GetSavedEventsQuery request, CancellationToken ct)
    {
        return await _db.SavedEvents
            .Where(s => s.UserId == request.UserId)
            .Include(s => s.Event).ThenInclude(e => e.Organizer)
            .Include(s => s.Event).ThenInclude(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Select(s => new EventResponseDto
            {
                Id = s.Event.Id, Title = s.Event.Title, Sport = s.Event.Sport,
                Location = s.Event.Location, Date = s.Event.Date,
                MaxParticipants = s.Event.MaxParticipants,
                OrganizerName = s.Event.Organizer.Username,
                ApprovedParticipantCount = s.Event.JoinRequests.Count(jr => jr.Status == "Approved")
            })
            .ToListAsync(ct);
    }
}

// ── Is Event Saved ──
public record IsEventSavedQuery(int EventId, int UserId) : IRequest<bool>;

public class IsEventSavedHandler : IRequestHandler<IsEventSavedQuery, bool>
{
    private readonly IAppDbContext _db;
    public IsEventSavedHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(IsEventSavedQuery request, CancellationToken ct)
    {
        return await _db.SavedEvents.AnyAsync(s => s.EventId == request.EventId && s.UserId == request.UserId, ct);
    }
}

// ── Report Event ──
public record ReportEventCommand(int EventId, int UserId, string Reason) : IRequest<bool>;

public class ReportEventHandler : IRequestHandler<ReportEventCommand, bool>
{
    private readonly IAppDbContext _db;
    public ReportEventHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(ReportEventCommand request, CancellationToken ct)
    {
        if (await _db.EventReports.AnyAsync(r => r.EventId == request.EventId && r.ReporterId == request.UserId, ct))
            return false;

        _db.EventReports.Add(new EventReport
        {
            EventId = request.EventId,
            ReporterId = request.UserId,
            Reason = request.Reason
        });
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

// ── Rate Event ──
public record RateEventCommand(int EventId, int UserId, CreateRatingDto Dto) : IRequest<RatingResponseDto?>;

public class RateEventHandler : IRequestHandler<RateEventCommand, RatingResponseDto?>
{
    private readonly IAppDbContext _db;
    public RateEventHandler(IAppDbContext db) => _db = db;

    public async Task<RatingResponseDto?> Handle(RateEventCommand request, CancellationToken ct)
    {
        if (await _db.EventRatings.AnyAsync(r => r.EventId == request.EventId && r.UserId == request.UserId, ct))
            return null;

        var rating = new EventRating
        {
            EventId = request.EventId,
            UserId = request.UserId,
            Score = Math.Clamp(request.Dto.Score, 1, 5),
            Comment = request.Dto.Comment
        };

        _db.EventRatings.Add(rating);
        await _db.SaveChangesAsync(ct);

        var user = await _db.Users.FindAsync([request.UserId], ct);
        return new RatingResponseDto
        {
            Id = rating.Id, EventId = rating.EventId, UserId = rating.UserId,
            Username = user?.Username ?? "Unknown", Score = rating.Score,
            Comment = rating.Comment, CreatedAt = rating.CreatedAt
        };
    }
}

// ── Get Ratings ──
public record GetRatingsQuery(int EventId) : IRequest<List<RatingResponseDto>>;

public class GetRatingsHandler : IRequestHandler<GetRatingsQuery, List<RatingResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetRatingsHandler(IAppDbContext db) => _db = db;

    public async Task<List<RatingResponseDto>> Handle(GetRatingsQuery request, CancellationToken ct)
    {
        return await _db.EventRatings
            .Where(r => r.EventId == request.EventId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingResponseDto
            {
                Id = r.Id, EventId = r.EventId, UserId = r.UserId,
                Username = r.User.Username, Score = r.Score,
                Comment = r.Comment, CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }
}
