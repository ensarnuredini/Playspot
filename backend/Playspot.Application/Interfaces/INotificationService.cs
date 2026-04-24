using Playspot.Application.DTOs.Notifications;

namespace Playspot.Application.Interfaces;

public interface INotificationService
{
    Task SendNotificationAsync(int userId, NotificationDto notification, CancellationToken ct = default);
}
