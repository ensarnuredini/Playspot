using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class EventService : IEventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventResponseDto>> GetAllAsync(string? sport, string? location)
    {
        var query = _context.Events
            .Include(e => e.Organiser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(sport))
            query = query.Where(e => e.Sport.ToLower().Contains(sport.ToLower()));

        if (!string.IsNullOrEmpty(location))
            query = query.Where(e => e.Location.ToLower().Contains(location.ToLower()));

        var events = await query
            .OrderByDescending(e => e.DateTime)
            .ToListAsync();

        return events.Select(e => new EventResponseDto
        {
            Id = e.Id,
            Sport = e.Sport,
            Title = e.Title,
            Location = e.Location,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            DateTime = e.DateTime,
            TotalSpots = e.TotalSpots,
            FilledSpots = e.FilledSpots,
            OrganiserId = e.OrganiserId,
            OrganiserUsername = e.Organiser.Username
        }).ToList();
    }

    public async Task<EventResponseDto?> GetByIdAsync(int id)
    {
        var evt = await _context.Events
            .Include(e => e.Organiser)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evt == null) return null;

        return new EventResponseDto
        {
            Id = evt.Id,
            Sport = evt.Sport,
            Title = evt.Title,
            Location = evt.Location,
            Latitude = evt.Latitude,
            Longitude = evt.Longitude,
            DateTime = evt.DateTime,
            TotalSpots = evt.TotalSpots,
            FilledSpots = evt.FilledSpots,
            OrganiserId = evt.OrganiserId,
            OrganiserUsername = evt.Organiser.Username
        };
    }

    public async Task<EventResponseDto> CreateAsync(CreateEventDto dto, int organiserId)
    {
        var evt = new Event
        {
            Sport = dto.Sport,
            Title = dto.Title,
            Location = dto.Location,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            DateTime = dto.DateTime,
            TotalSpots = dto.TotalSpots,
            OrganiserId = organiserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(evt);
        await _context.SaveChangesAsync();

        var organiser = await _context.Users.FindAsync(organiserId);

        return new EventResponseDto
        {
            Id = evt.Id,
            Sport = evt.Sport,
            Title = evt.Title,
            Location = evt.Location,
            Latitude = evt.Latitude,
            Longitude = evt.Longitude,
            DateTime = evt.DateTime,
            TotalSpots = evt.TotalSpots,
            FilledSpots = evt.FilledSpots,
            OrganiserId = evt.OrganiserId,
            OrganiserUsername = organiser!.Username
        };
    }

    public async Task DeleteAsync(int id, int requestingUserId)
    {
        var evt = await _context.Events.FindAsync(id);

        if (evt == null)
            throw new InvalidOperationException("Event not found");

        if (evt.OrganiserId != requestingUserId)
            throw new UnauthorizedAccessException("Can only delete your own events");

        _context.Events.Remove(evt);
        await _context.SaveChangesAsync();
    }
}
