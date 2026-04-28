using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.JoinRequests;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Application.DTOs.Notifications;

namespace Playspot.Application.Features.JoinRequests;

// ── Request to Join ──
public record RequestToJoinCommand(int EventId, int UserId) : IRequest<JoinRequestResponseDto?>;

public class RequestToJoinHandler : IRequestHandler<RequestToJoinCommand, JoinRequestResponseDto?>
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _notificationService;
    
    public RequestToJoinHandler(IAppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<JoinRequestResponseDto?> Handle(RequestToJoinCommand request, CancellationToken ct)
    {
        var ev = await _db.Events.FindAsync([request.EventId], ct);
        if (ev == null) return null;

        var exists = await _db.JoinRequests
            .AnyAsync(jr => jr.EventId == request.EventId && jr.UserId == request.UserId, ct);
        if (exists) return null;

        var jr = new JoinRequest
        {
            EventId = request.EventId,
            UserId = request.UserId,
            Status = ev.RequiresApproval ? "Pending" : "Approved"
        };

        _db.JoinRequests.Add(jr);

        var user = await _db.Users.FindAsync([request.UserId], ct);
        string username = user?.Username ?? "Unknown";

        // Create and send notification
        var action = ev.RequiresApproval ? "requested to join" : "joined";
        var notification = new Notification
        {
            UserId = ev.OrganizerId,
            Message = $"{username} {action} '{ev.Title}'.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(ct);

        var notificationDto = new NotificationDto
        {
            Id = notification.Id,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedDateFormatted = notification.CreatedAt.ToString("MMM dd, HH:mm")
        };
        
        await _notificationService.SendNotificationAsync(ev.OrganizerId, notificationDto, ct);

        return MapToDto(jr, username);
    }

    private static JoinRequestResponseDto MapToDto(JoinRequest jr, string username) => new()
    {
        Id = jr.Id, EventId = jr.EventId, UserId = jr.UserId,
        Username = username, Status = jr.Status, RequestedAt = jr.RequestedAt
    };
}

// ── Get Requests for Event ──
public record GetJoinRequestsQuery(int EventId, int OrganizerId) : IRequest<List<JoinRequestResponseDto>>;

public class GetJoinRequestsHandler : IRequestHandler<GetJoinRequestsQuery, List<JoinRequestResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetJoinRequestsHandler(IAppDbContext db) => _db = db;

    public async Task<List<JoinRequestResponseDto>> Handle(GetJoinRequestsQuery request, CancellationToken ct)
    {
        var ev = await _db.Events.FindAsync([request.EventId], ct);
        if (ev == null || ev.OrganizerId != request.OrganizerId) return [];

        return await _db.JoinRequests
            .Where(jr => jr.EventId == request.EventId)
            .Include(jr => jr.User)
            .Select(jr => new JoinRequestResponseDto
            {
                Id = jr.Id, EventId = jr.EventId, UserId = jr.UserId,
                Username = jr.User.Username, Status = jr.Status, RequestedAt = jr.RequestedAt
            })
            .ToListAsync(ct);
    }
}

// ── Update Status ──
public record UpdateJoinRequestStatusCommand(int RequestId, string Status, int OrganizerId) : IRequest<bool>;

public class UpdateJoinRequestStatusHandler : IRequestHandler<UpdateJoinRequestStatusCommand, bool>
{
    private readonly IAppDbContext _db;
    private readonly INotificationService _notificationService;

    public UpdateJoinRequestStatusHandler(IAppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<bool> Handle(UpdateJoinRequestStatusCommand request, CancellationToken ct)
    {
        var jr = await _db.JoinRequests
            .Include(jr => jr.Event)
            .FirstOrDefaultAsync(jr => jr.Id == request.RequestId, ct);

        if (jr == null || jr.Event.OrganizerId != request.OrganizerId) return false;

        jr.Status = request.Status;
        
        if (request.Status == "Approved")
        {
            var notification = new Notification
            {
                UserId = jr.UserId,
                Message = $"Your request to join '{jr.Event.Title}' has been approved!",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _db.Notifications.Add(notification);
        }

        await _db.SaveChangesAsync(ct);

        if (request.Status == "Approved")
        {
            // The newest notification will be the one we just saved
            var savedNotification = await _db.Notifications
                .Where(n => n.UserId == jr.UserId)
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (savedNotification != null)
            {
                var notificationDto = new NotificationDto
                {
                    Id = savedNotification.Id,
                    Message = savedNotification.Message,
                    IsRead = savedNotification.IsRead,
                    CreatedDateFormatted = savedNotification.CreatedAt.ToString("MMM dd, HH:mm")
                };
                await _notificationService.SendNotificationAsync(jr.UserId, notificationDto, ct);
            }
        }

        return true;
    }
}

// ── Withdraw ──
public record WithdrawJoinRequestCommand(int EventId, int UserId) : IRequest<bool>;

public class WithdrawJoinRequestHandler : IRequestHandler<WithdrawJoinRequestCommand, bool>
{
    private readonly IAppDbContext _db;
    public WithdrawJoinRequestHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(WithdrawJoinRequestCommand request, CancellationToken ct)
    {
        var jr = await _db.JoinRequests
            .FirstOrDefaultAsync(j => j.EventId == request.EventId && j.UserId == request.UserId, ct);

        if (jr == null) return false;
        _db.JoinRequests.Remove(jr);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
