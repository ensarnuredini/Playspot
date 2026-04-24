namespace Playspot.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string CreatedDateFormatted { get; set; } = string.Empty;
}
