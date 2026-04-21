using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Events;
using Playspot.Application.Features.Events.Commands;
using Playspot.Application.Features.Events.Queries;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;
    public EventController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EventFilterDto filters)
        => Ok(await _mediator.Send(new GetFilteredEventsQuery(filters)));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _mediator.Send(new GetEventByIdQuery(id));
        return ev == null ? NotFound() : Ok(ev);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new CreateEventCommand(dto, userId));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEventDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new UpdateEventCommand(id, dto, userId));
        return result == null ? Forbid() : Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return await _mediator.Send(new DeleteEventCommand(id, userId)) ? NoContent() : Forbid();
    }

    [Authorize]
    [HttpGet("my/hosting")]
    public async Task<IActionResult> GetMyHosting()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _mediator.Send(new GetMyHostingQuery(userId)));
    }

    [Authorize]
    [HttpGet("my/joined")]
    public async Task<IActionResult> GetMyJoined()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _mediator.Send(new GetMyJoinedQuery(userId)));
    }

    [Authorize]
    [HttpGet("my/past")]
    public async Task<IActionResult> GetMyPast()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _mediator.Send(new GetMyPastQuery(userId)));
    }

    [HttpGet("{id}/similar")]
    public async Task<IActionResult> GetSimilar(int id)
        => Ok(await _mediator.Send(new GetSimilarEventsQuery(id)));
}