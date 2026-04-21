using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Comments;
using Playspot.Application.Features.Comments;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly IMediator _mediator;
    public CommentController(IMediator mediator) => _mediator = mediator;

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetComments(int eventId)
        => Ok(await _mediator.Send(new GetCommentsQuery(eventId)));

    [Authorize]
    [HttpPost("event/{eventId}")]
    public async Task<IActionResult> AddComment(int eventId, CreateCommentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _mediator.Send(new AddCommentCommand(eventId, userId, dto));
        return Created($"api/comment/{result.Id}", result);
    }
}
