using Microsoft.AspNetCore.SignalR;
using Playspot.API.Hubs;
using Playspot.Application.DTOs.Notifications;
using Playspot.Application.Interfaces;

namespace Playspot.API.Services;

public class NotificationHubService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(int userId, NotificationDto notification, CancellationToken ct = default)
    {
        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification, ct);
    }
}
