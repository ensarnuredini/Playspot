using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Ratings;
using Playspot.Application.DTOs.Reports;
using Playspot.Application.Interfaces;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/event")]
[Authorize]
public class EventActionsController : ControllerBase
{
    private readonly ISavedEventService _saved;
    private readonly IReportService _report;
    private readonly IRatingService _rating;

    public EventActionsController(ISavedEventService saved, IReportService report, IRatingService rating)
    {
        _saved = saved;
        _report = report;
        _rating = rating;
    }

    // ── Save / Unsave (Bookmark) ──

    [HttpPost("{eventId}/save")]
    public async Task<IActionResult> SaveEvent(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _saved.SaveAsync(eventId, userId) ? Ok() : BadRequest("Already saved.");
    }

    [HttpDelete("{eventId}/save")]
    public async Task<IActionResult> UnsaveEvent(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _saved.UnsaveAsync(eventId, userId) ? NoContent() : NotFound();
    }

    [HttpGet("saved")]
    public async Task<IActionResult> GetSaved()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _saved.GetSavedByUserAsync(userId));
    }

    [HttpGet("{eventId}/saved")]
    public async Task<IActionResult> IsEventSaved(int eventId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(new { isSaved = await _saved.IsEventSavedAsync(eventId, userId) });
    }

    // ── Report ──

    [HttpPost("{eventId}/report")]
    public async Task<IActionResult> ReportEvent(int eventId, CreateReportDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _report.ReportEventAsync(eventId, userId, dto.Reason)
            ? Ok() : BadRequest("Already reported.");
    }

    // ── Rating ──

    [HttpPost("{eventId}/rate")]
    public async Task<IActionResult> RateEvent(int eventId, CreateRatingDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _rating.RateEventAsync(eventId, userId, dto);
        return result == null ? BadRequest("Already rated.") : Created($"api/event/{eventId}/ratings", result);
    }

    [AllowAnonymous]
    [HttpGet("{eventId}/ratings")]
    public async Task<IActionResult> GetRatings(int eventId)
    {
        return Ok(await _rating.GetRatingsForEventAsync(eventId));
    }
}
