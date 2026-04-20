using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.Interfaces;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JoinRequestController : ControllerBase
{
    private readonly IJoinRequestService _joinRequest;
    public JoinRequestController(IJoinRequestService joinRequest) => _joinRequest = joinRequest;

    [HttpPost("{eventId}")]
    public async Task<IActionResult> RequestToJoin(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _joinRequest.RequestToJoinAsync(eventId, userId);
        return result == null ? BadRequest("Already requested or event not found.") : Ok(result);
    }

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetRequests(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _joinRequest.GetRequestsForEventAsync(eventId, userId);
        return Ok(result);
    }

    [HttpPatch("{requestId}/status")]
    public async Task<IActionResult> UpdateStatus(int requestId, [FromBody] string status)
    {
        if (status != "Approved" && status != "Rejected")
            return BadRequest("Status must be 'Approved' or 'Rejected'.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _joinRequest.UpdateStatusAsync(requestId, status, userId) 
            ? NoContent() : Forbid();
    }

    [HttpDelete("{eventId}")]
    public async Task<IActionResult> Withdraw(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _joinRequest.WithdrawAsync(eventId, userId)
            ? NoContent() : NotFound();
    }
}