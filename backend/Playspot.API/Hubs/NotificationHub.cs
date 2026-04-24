using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Playspot.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    // Connections are automatically mapped to User.Identity.Name in ASP.NET Core SignalR
    // so we can use Clients.User(userId) directly to send specific notifications.
}
