using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.Features.Notifications;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator) => _mediator = mediator;

    private int GetCurrentUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetCurrentUserId();
        var notifications = await _mediator.Send(new GetNotificationsQuery(userId));
        return Ok(notifications);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _mediator.Send(new MarkNotificationReadCommand(id, userId));
        if (!result) return NotFound(new { error = "Notification not found or unauthorized" });
        
        return Ok(new { success = true });
    }
}
