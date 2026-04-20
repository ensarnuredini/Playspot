using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.JoinRequests;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class JoinRequestService : IJoinRequestService
{
    private readonly AppDbContext _db;

    public JoinRequestService(AppDbContext db) => _db = db;

    public async Task<JoinRequestResponseDto?> RequestToJoinAsync(int eventId, int userId)
    {
        var alreadyRequested = await _db.JoinRequests
            .AnyAsync(jr => jr.EventId == eventId && jr.UserId == userId);

        if (alreadyRequested) return null;

        var jr = new JoinRequest { EventId = eventId, UserId = userId };
        _db.JoinRequests.Add(jr);
        await _db.SaveChangesAsync();

        return MapToDto(jr);
    }

    public async Task<List<JoinRequestResponseDto>> GetRequestsForEventAsync(int eventId, int organizerId)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null || ev.OrganizerId != organizerId) return new();

        return await _db.JoinRequests
            .Where(jr => jr.EventId == eventId)
            .Include(jr => jr.User)
            .Select(jr => MapToDto(jr))
            .ToListAsync();
    }

    public async Task<bool> UpdateStatusAsync(int requestId, string status, int organizerId)
    {
        var jr = await _db.JoinRequests
            .Include(jr => jr.Event)
            .FirstOrDefaultAsync(jr => jr.Id == requestId);

        if (jr == null || jr.Event.OrganizerId != organizerId) return false;

        jr.Status = status;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WithdrawAsync(int eventId, int userId)
    {
        var jr = await _db.JoinRequests
            .FirstOrDefaultAsync(jr => jr.EventId == eventId && jr.UserId == userId);

        if (jr == null) return false;

        _db.JoinRequests.Remove(jr);
        await _db.SaveChangesAsync();
        return true;
    }

    private static JoinRequestResponseDto MapToDto(JoinRequest jr) => new()
    {
        Id = jr.Id,
        EventId = jr.EventId,
        UserId = jr.UserId,
        Username = jr.User?.Username ?? "Unknown",
        Status = jr.Status,
        RequestedAt = jr.RequestedAt
    };
}