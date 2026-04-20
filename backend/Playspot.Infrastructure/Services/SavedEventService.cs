using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class SavedEventService : ISavedEventService
{
    private readonly AppDbContext _db;

    public SavedEventService(AppDbContext db) => _db = db;

    public async Task<bool> SaveAsync(int eventId, int userId)
    {
        var exists = await _db.SavedEvents
            .AnyAsync(s => s.EventId == eventId && s.UserId == userId);

        if (exists) return false;

        _db.SavedEvents.Add(new SavedEvent
        {
            EventId = eventId,
            UserId = userId
        });

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnsaveAsync(int eventId, int userId)
    {
        var saved = await _db.SavedEvents
            .FirstOrDefaultAsync(s => s.EventId == eventId && s.UserId == userId);

        if (saved == null) return false;

        _db.SavedEvents.Remove(saved);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<EventResponseDto>> GetSavedByUserAsync(int userId)
    {
        return await _db.SavedEvents
            .Where(s => s.UserId == userId)
            .Include(s => s.Event).ThenInclude(e => e.Organizer)
            .Include(s => s.Event).ThenInclude(e => e.JoinRequests).ThenInclude(jr => jr.User)
            .Select(s => new EventResponseDto
            {
                Id = s.Event.Id,
                Title = s.Event.Title,
                Sport = s.Event.Sport,
                Location = s.Event.Location,
                Date = s.Event.Date,
                MaxParticipants = s.Event.MaxParticipants,
                OrganizerName = s.Event.Organizer.Username,
                ApprovedParticipantCount = s.Event.JoinRequests.Count(jr => jr.Status == "Approved")
            })
            .ToListAsync();
    }

    public async Task<bool> IsEventSavedAsync(int eventId, int userId)
    {
        return await _db.SavedEvents
            .AnyAsync(s => s.EventId == eventId && s.UserId == userId);
    }
}
