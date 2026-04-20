using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Interfaces;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IEventService _events;
    public EventController(IEventService events) => _events = events;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventFilterDto filters)
    {
        var result = await _events.GetFilteredAsync(filters);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _events.GetByIdAsync(id);
        return ev == null ? NotFound() : Ok(ev);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _events.CreateAsync(dto, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _events.UpdateAsync(id, dto, userId);
        return result == null ? Forbid() : Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _events.DeleteAsync(id, userId) ? NoContent() : Forbid();
    }

    [Authorize]
    [HttpGet("my/hosting")]
    public async Task<IActionResult> GetMyHosting()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _events.GetMyHostingAsync(userId));
    }

    [Authorize]
    [HttpGet("my/joined")]
    public async Task<IActionResult> GetMyJoined()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _events.GetMyJoinedAsync(userId));
    }

    [Authorize]
    [HttpGet("my/past")]
    public async Task<IActionResult> GetMyPast()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _events.GetMyPastAsync(userId));
    }

    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilar(int id)
    {
        return Ok(await _events.GetSimilarAsync(id));
    }
}