using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Comments;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _db;

    public CommentService(AppDbContext db) => _db = db;

    public async Task<List<CommentResponseDto>> GetCommentsForEventAsync(int eventId)
    {
        var ev = await _db.Events.FindAsync(eventId);

        return await _db.Comments
            .Where(c => c.EventId == eventId)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                EventId = c.EventId,
                UserId = c.UserId,
                Username = c.User.Username,
                Text = c.Text,
                IsHost = ev != null && c.UserId == ev.OrganizerId,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentResponseDto> AddCommentAsync(int eventId, int userId, CreateCommentDto dto)
    {
        var comment = new Comment
        {
            EventId = eventId,
            UserId = userId,
            Text = dto.Text
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        var ev = await _db.Events.FindAsync(eventId);

        return new CommentResponseDto
        {
            Id = comment.Id,
            EventId = comment.EventId,
            UserId = comment.UserId,
            Username = user?.Username ?? "Unknown",
            Text = comment.Text,
            IsHost = ev != null && userId == ev.OrganizerId,
            CreatedAt = comment.CreatedAt
        };
    }
}
