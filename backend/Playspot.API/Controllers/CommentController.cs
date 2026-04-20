using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Playspot.Application.DTOs.Comments;
using Playspot.Application.Interfaces;
using System.Security.Claims;

namespace Playspot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _comments;
    public CommentController(ICommentService comments) => _comments = comments;

    [HttpGet("event/{eventId}")]
    public async Task<IActionResult> GetComments(int eventId)
    {
        return Ok(await _comments.GetCommentsForEventAsync(eventId));
    }

    [Authorize]
    [HttpPost("event/{eventId}")]
    public async Task<IActionResult> AddComment(int eventId, CreateCommentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _comments.AddCommentAsync(eventId, userId, dto);
        return Created($"api/comment/{result.Id}", result);
    }
}
