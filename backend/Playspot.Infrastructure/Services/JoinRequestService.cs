using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.JoinRequests;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class JoinRequestService : IJoinRequestService
{
    private readonly AppDbContext _context;

    public JoinRequestService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JoinRequestResponseDto> RequestJoinAsync(int eventId, int userId)
    {
        // Check if event exists
        var evt = await _context.Events.FindAsync(eventId);
        if (evt == null)
            throw new InvalidOperationException("Event not found");

        // Check if user already requested to join
        var existingRequest = await _context.JoinRequests
            .FirstOrDefaultAsync(jr => jr.EventId == eventId && jr.UserId == userId);

        if (existingRequest != null)
            throw new InvalidOperationException("User already requested to join this event");

        // Check if event is full
        if (evt.FilledSpots >= evt.TotalSpots)
            throw new InvalidOperationException("Event is full");

        var joinRequest = new JoinRequest
        {
            EventId = eventId,
            UserId = userId,
            Status = JoinStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _context.JoinRequests.Add(joinRequest);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        var ev = await _context.Events.FindAsync(eventId);

        return new JoinRequestResponseDto
        {
            Id = joinRequest.Id,
            EventId = joinRequest.EventId,
            EventTitle = ev!.Title,
            UserId = joinRequest.UserId,
            Username = user!.Username,
            Status = joinRequest.Status.ToString(),
            RequestedAt = joinRequest.RequestedAt
        };
    }

    public async Task<JoinRequestResponseDto> UpdateStatusAsync(int requestId, string status, int organiserId)
    {
        var joinRequest = await _context.JoinRequests
            .Include(jr => jr.Event)
            .Include(jr => jr.User)
            .FirstOrDefaultAsync(jr => jr.Id == requestId);

        if (joinRequest == null)
            throw new InvalidOperationException("Join request not found");

        if (joinRequest.Event.OrganiserId != organiserId)
            throw new UnauthorizedAccessException("You can only update requests for your events");

        // Parse status
        if (!Enum.TryParse<JoinStatus>(status, true, out var newStatus))
            throw new InvalidOperationException("Invalid status");

        var oldStatus = joinRequest.Status;

        joinRequest.Status = newStatus;
        _context.JoinRequests.Update(joinRequest);

        // Update FilledSpots if approved
        if (oldStatus != JoinStatus.Approved && newStatus == JoinStatus.Approved)
        {
            joinRequest.Event.FilledSpots++;
        }
        // Reduce FilledSpots if was approved and now cancelled
        else if (oldStatus == JoinStatus.Approved && newStatus != JoinStatus.Approved)
        {
            joinRequest.Event.FilledSpots--;
        }

        _context.Events.Update(joinRequest.Event);
        await _context.SaveChangesAsync();

        return new JoinRequestResponseDto
        {
            Id = joinRequest.Id,
            EventId = joinRequest.EventId,
            EventTitle = joinRequest.Event.Title,
            UserId = joinRequest.UserId,
            Username = joinRequest.User.Username,
            Status = joinRequest.Status.ToString(),
            RequestedAt = joinRequest.RequestedAt
        };
    }

    public async Task<List<JoinRequestResponseDto>> GetRequestsForEventAsync(int eventId, int organiserId)
    {
        var evt = await _context.Events.FindAsync(eventId);

        if (evt == null)
            throw new InvalidOperationException("Event not found");

        if (evt.OrganiserId != organiserId)
            throw new UnauthorizedAccessException("You can only view requests for your events");

        var requests = await _context.JoinRequests
            .Include(jr => jr.User)
            .Include(jr => jr.Event)
            .Where(jr => jr.EventId == eventId)
            .OrderByDescending(jr => jr.RequestedAt)
            .ToListAsync();

        return requests.Select(jr => new JoinRequestResponseDto
        {
            Id = jr.Id,
            EventId = jr.EventId,
            EventTitle = jr.Event.Title,
            UserId = jr.UserId,
            Username = jr.User.Username,
            Status = jr.Status.ToString(),
            RequestedAt = jr.RequestedAt
        }).ToList();
    }
}
