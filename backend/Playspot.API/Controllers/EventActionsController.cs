using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Ratings;
using Playspot.Application.DTOs.Reports;
using Playspot.Application.Features.EventActions;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/event")]
[Authorize]
public class EventActionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public EventActionsController(IMediator mediator) => _mediator = mediator;

    // ── Save / Unsave ──

    [HttpPost("{eventId}/save")]
    public async Task<IActionResult> SaveEvent(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _mediator.Send(new SaveEventCommand(eventId, userId)) ? Ok() : BadRequest("Already saved.");
    }

    [HttpDelete("{eventId}/save")]
    public async Task<IActionResult> UnsaveEvent(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _mediator.Send(new UnsaveEventCommand(eventId, userId)) ? NoContent() : NotFound();
    }

    [HttpGet("saved")]
    public async Task<IActionResult> GetSaved()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _mediator.Send(new GetSavedEventsQuery(userId)));
    }

    [HttpGet("{eventId}/saved")]
    public async Task<IActionResult> IsEventSaved(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new { isSaved = await _mediator.Send(new IsEventSavedQuery(eventId, userId)) });
    }

    // ── Report ──

    [HttpPost("{eventId}/report")]
    public async Task<IActionResult> ReportEvent(int eventId, CreateReportDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _mediator.Send(new ReportEventCommand(eventId, userId, dto.Reason))
            ? Ok() : BadRequest("Already reported.");
    }

    // ── Rating ──

    [HttpPost("{eventId}/rate")]
    public async Task<IActionResult> RateEvent(int eventId, CreateRatingDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new RateEventCommand(eventId, userId, dto));
        return result == null ? BadRequest("Already rated.") : Created($"api/event/{eventId}/ratings", result);
    }

    [AllowAnonymous]
    [HttpGet("{eventId}/ratings")]
    public async Task<IActionResult> GetRatings(int eventId)
        => Ok(await _mediator.Send(new GetRatingsQuery(eventId)));
}
