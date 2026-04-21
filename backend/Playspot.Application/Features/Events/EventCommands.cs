using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;

namespace Playspot.Application.Features.Events.Commands;

// ── Create Event ──
public record CreateEventCommand(CreateEventDto Dto, int OrganizerId) : IRequest<EventResponseDto>;

public class CreateEventHandler : IRequestHandler<CreateEventCommand, EventResponseDto>
{
    private readonly IAppDbContext _db;
    public CreateEventHandler(IAppDbContext db) => _db = db;

    public async Task<EventResponseDto> Handle(CreateEventCommand request, CancellationToken ct)
    {
        var dto = request.Dto;
        var ev = new Event
        {
            Title = dto.Title,
            Sport = dto.Sport,
            Description = dto.Description,
            Location = dto.Location,
            Date = dto.DateTime,
            MaxParticipants = dto.TotalSpots,
            DurationMinutes = dto.DurationMinutes,
            SkillLevel = dto.SkillLevel,
            Gender = dto.Gender,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            RequiresApproval = dto.RequiresApproval,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            OrganizerId = request.OrganizerId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync(ct);

        // Load organizer for DTO mapping
        await _db.Events.Entry(ev).Reference(e => e.Organizer).LoadAsync(ct);

        return EventMapper.MapToDto(ev);
    }
}

// ── Update Event ──
public record UpdateEventCommand(int EventId, UpdateEventDto Dto, int RequestingUserId) : IRequest<EventResponseDto?>;

public class UpdateEventHandler : IRequestHandler<UpdateEventCommand, EventResponseDto?>
{
    private readonly IAppDbContext _db;
    public UpdateEventHandler(IAppDbContext db) => _db = db;

    public async Task<EventResponseDto?> Handle(UpdateEventCommand request, CancellationToken ct)
    {
        var ev = await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .FirstOrDefaultAsync(e => e.Id == request.EventId, ct);

        if (ev == null || ev.OrganizerId != request.RequestingUserId) return null;

        var dto = request.Dto;
        ev.Title = dto.Title;
        ev.Sport = dto.Sport;
        ev.Description = dto.Description;
        ev.Location = dto.Location;
        ev.Date = dto.DateTime;
        ev.MaxParticipants = dto.TotalSpots;
        ev.DurationMinutes = dto.DurationMinutes;
        ev.SkillLevel = dto.SkillLevel;
        ev.Gender = dto.Gender;
        ev.MinAge = dto.MinAge;
        ev.MaxAge = dto.MaxAge;
        ev.RequiresApproval = dto.RequiresApproval;
        ev.Latitude = dto.Latitude;
        ev.Longitude = dto.Longitude;

        await _db.SaveChangesAsync(ct);
        return EventMapper.MapToDto(ev);
    }
}

// ── Delete Event ──
public record DeleteEventCommand(int EventId, int RequestingUserId) : IRequest<bool>;

public class DeleteEventHandler : IRequestHandler<DeleteEventCommand, bool>
{
    private readonly IAppDbContext _db;
    public DeleteEventHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken ct)
    {
        var ev = await _db.Events.FindAsync([request.EventId], ct);
        if (ev == null || ev.OrganizerId != request.RequestingUserId) return false;

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
