using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Notifications;
using Playspot.Application.Interfaces;

namespace Playspot.Application.Features.Notifications;

// ── Get Notifications ──
public record GetNotificationsQuery(int UserId) : IRequest<List<NotificationDto>>;

public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, List<NotificationDto>>
{
    private readonly IAppDbContext _db;
    public GetNotificationsHandler(IAppDbContext db) => _db = db;

    public async Task<List<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        return await _db.Notifications
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDateFormatted = n.CreatedAt.ToString("MMM dd, HH:mm")
            })
            .ToListAsync(ct);
    }
}

// ── Mark As Read ──
public record MarkNotificationReadCommand(int NotificationId, int UserId) : IRequest<bool>;

public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, bool>
{
    private readonly IAppDbContext _db;
    public MarkNotificationReadHandler(IAppDbContext db) => _db = db;

    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notif = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId, ct);

        if (notif == null) return false;

        notif.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
