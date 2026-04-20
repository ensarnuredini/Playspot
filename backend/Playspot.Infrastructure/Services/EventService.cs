using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _db;

    public EventService(AppDbContext db) => _db = db;

    public async Task<List<EventResponseDto>> GetAllAsync()
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.Status == "Active")
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<List<EventResponseDto>> GetFilteredAsync(EventFilterDto filters)
    {
        var query = _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.Status == "Active")
            .AsQueryable();

        // Filter by sport
        if (!string.IsNullOrEmpty(filters.Sport))
            query = query.Where(e => e.Sport.ToLower() == filters.Sport.ToLower());

        // Filter by skill level
        if (!string.IsNullOrEmpty(filters.SkillLevel))
            query = query.Where(e => e.SkillLevel.ToLower() == filters.SkillLevel.ToLower());

        // Filter by date
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

        // Search by title or location
        if (!string.IsNullOrEmpty(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(e =>
                e.Title.ToLower().Contains(search) ||
                e.Location.ToLower().Contains(search) ||
                e.Sport.ToLower().Contains(search));
        }

        // Filter by open spots
        if (filters.HasOpenSpots == true)
            query = query.Where(e => e.JoinRequests.Count(jr => jr.Status == "Approved") < e.MaxParticipants);

        // Sort
        query = filters.SortBy?.ToLower() switch
        {
            "date" => query.OrderBy(e => e.Date),
            "spots" => query.OrderByDescending(e => e.MaxParticipants - e.JoinRequests.Count(jr => jr.Status == "Approved")),
            "popular" => query.OrderByDescending(e => e.JoinRequests.Count(jr => jr.Status == "Approved")),
            _ => query.OrderBy(e => e.Date) // default: soonest first
        };

        // Pagination
        var total = await query.CountAsync();
        var events = await query
            .Skip((filters.Page - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return events;
    }

    public async Task<EventResponseDto?> GetByIdAsync(int id)
    {
        var e = await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        return e == null ? null : MapToDto(e);
    }

    public async Task<EventResponseDto> CreateAsync(CreateEventDto dto, int organizerId)
    {
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
            OrganizerId = organizerId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        await _db.Entry(ev).Reference(e => e.Organizer).LoadAsync();

        return MapToDto(ev);
    }

    public async Task<EventResponseDto?> UpdateAsync(int id, UpdateEventDto dto, int requestingUserId)
    {
        var ev = await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null || ev.OrganizerId != requestingUserId) return null;

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

        await _db.SaveChangesAsync();
        return MapToDto(ev);
    }

    public async Task<bool> DeleteAsync(int id, int requestingUserId)
    {
        var ev = await _db.Events.FindAsync(id);
        if (ev == null || ev.OrganizerId != requestingUserId) return false;

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<EventResponseDto>> GetMyHostingAsync(int userId)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.OrganizerId == userId && e.Status == "Active")
            .OrderBy(e => e.Date)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<List<EventResponseDto>> GetMyJoinedAsync(int userId)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.JoinRequests.Any(jr => jr.UserId == userId && jr.Status == "Approved")
                        && e.Status == "Active"
                        && e.Date >= DateTime.UtcNow)
            .OrderBy(e => e.Date)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<List<EventResponseDto>> GetMyPastAsync(int userId)
    {
        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => (e.OrganizerId == userId ||
                         e.JoinRequests.Any(jr => jr.UserId == userId && jr.Status == "Approved"))
                        && (e.Date < DateTime.UtcNow || e.Status == "Completed"))
            .OrderByDescending(e => e.Date)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    public async Task<List<EventResponseDto>> GetSimilarAsync(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return new();

        return await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Where(e => e.Id != eventId
                        && e.Sport == ev.Sport
                        && e.Status == "Active"
                        && e.Date >= DateTime.UtcNow)
            .OrderBy(e => e.Date)
            .Take(5)
            .Select(e => MapToDto(e))
            .ToListAsync();
    }

    private static EventResponseDto MapToDto(Event e) => new()
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
        ApprovedParticipantCount = e.JoinRequests.Count(jr => jr.Status == "Approved"),
        Participants = e.JoinRequests
            .Where(jr => jr.Status == "Approved")
            .Select(jr => new ParticipantDto
            {
                UserId = jr.UserId,
                Username = jr.User?.Username ?? "Unknown",
                IsHost = jr.UserId == e.OrganizerId
            }).ToList()
    };
}