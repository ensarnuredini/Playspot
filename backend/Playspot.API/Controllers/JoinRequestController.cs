using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.Features.JoinRequests;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JoinRequestController : ControllerBase
{
    private readonly IMediator _mediator;
    public JoinRequestController(IMediator mediator) => _mediator = mediator;

    [HttpPost("{eventId}")]
    public async Task<IActionResult> RequestToJoin(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new RequestToJoinCommand(eventId, userId));
        return result == null ? BadRequest("Cannot join this event.") : Ok(result);
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetRequests(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _mediator.Send(new GetJoinRequestsQuery(eventId, userId)));
    }

    [HttpPatch("{requestId}/status")]
    public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] string status)
    {
        if (status != "Approved" && status != "Rejected")
            return BadRequest("Status must be 'Approved' or 'Rejected'.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _mediator.Send(new UpdateJoinRequestStatusCommand(requestId, status, userId))
            ? NoContent() : Forbid();
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> Withdraw(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _mediator.Send(new WithdrawJoinRequestCommand(eventId, userId))
            ? NoContent() : NotFound();
    }
}