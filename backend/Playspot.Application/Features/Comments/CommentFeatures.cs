using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Comments;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;

namespace Playspot.Application.Features.Comments;

// ── Get Comments ──
public record GetCommentsQuery(int EventId) : IRequest<List<CommentResponseDto>>;

public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, List<CommentResponseDto>>
{
    private readonly IAppDbContext _db;
    public GetCommentsHandler(IAppDbContext db) => _db = db;

    public async Task<List<CommentResponseDto>> Handle(GetCommentsQuery request, CancellationToken ct)
    {
        var ev = await _db.Events.FindAsync([request.EventId], ct);

        return await _db.Comments
            .Where(c => c.EventId == request.EventId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id, EventId = c.EventId, UserId = c.UserId,
                Username = c.User.Username, Text = c.Text,
                IsHost = ev != null && c.UserId == ev.OrganizerId,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);
    }
}

// ── Add Comment ──
public record AddCommentCommand(int EventId, int UserId, CreateCommentDto Dto) : IRequest<CommentResponseDto>;

public class AddCommentHandler : IRequestHandler<AddCommentCommand, CommentResponseDto>
{
    private readonly IAppDbContext _db;
    public AddCommentHandler(IAppDbContext db) => _db = db;

    public async Task<CommentResponseDto> Handle(AddCommentCommand request, CancellationToken ct)
    {
        var comment = new Comment
        {
            EventId = request.EventId,
            UserId = request.UserId,
            Text = request.Dto.Text
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync(ct);

        var user = await _db.Users.FindAsync([request.UserId], ct);
        var ev = await _db.Events.FindAsync([request.EventId], ct);

        return new CommentResponseDto
        {
            Id = comment.Id, EventId = comment.EventId, UserId = comment.UserId,
            Username = user?.Username ?? "Unknown", Text = comment.Text,
            IsHost = ev != null && request.UserId == ev.OrganizerId,
            CreatedAt = comment.CreatedAt
        };
    }
}
