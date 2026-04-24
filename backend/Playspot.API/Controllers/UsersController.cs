using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Users;
using Playspot.Application.Features.Users.Commands;
using Playspot.Application.Features.Users.Queries;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserProfile(int id)
    {
        var profile = await _mediator.Send(new GetUserProfileQuery(id));
        return profile == null ? NotFound() : Ok(profile);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProfile(int id, UpdateProfileDto dto)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var currentUserId) || currentUserId != id)
        {
            return Forbid();
        }

        var result = await _mediator.Send(new UpdateProfileCommand(id, dto));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("{id}/events")]
    public async Task<IActionResult> GetUserEvents(int id)
    {
        var events = await _mediator.Send(new GetUserEventsQuery(id));
        return Ok(events);
    }

    [HttpGet("{id}/joined-events")]
    public async Task<IActionResult> GetUserJoinedEvents(int id)
    {
        var events = await _mediator.Send(new GetUserJoinedEventsQuery(id));
        return Ok(events);
    }
}
