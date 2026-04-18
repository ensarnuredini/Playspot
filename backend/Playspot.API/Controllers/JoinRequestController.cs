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
<<<<<<< HEAD
        var result = await _joinRequest.RequestToJoinAsync(eventId, userId);
        return result == null ? BadRequest("Already requested or event not found.") : Ok(result);
=======
        try
        {
            var result = await _joinRequest.RequestJoinAsync(eventId, userId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
>>>>>>> f781fd9 (feat: JavaScript frontend implementation with API integration, dashboard with Leaflet map, create-event form, authentication, and CSS refinements)
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
<<<<<<< HEAD
        if (status != "Approved" && status != "Rejected")
            return BadRequest("Status must be 'Approved' or 'Rejected'.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _joinRequest.UpdateStatusAsync(requestId, status, userId) 
            ? NoContent() : Forbid();
=======
        if (status != "Approved" && status != "Declined")
            return BadRequest("Status must be 'Approved' or 'Declined'.");

        try
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _joinRequest.UpdateStatusAsync(requestId, status, userId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
>>>>>>> f781fd9 (feat: JavaScript frontend implementation with API integration, dashboard with Leaflet map, create-event form, authentication, and CSS refinements)
    }
}