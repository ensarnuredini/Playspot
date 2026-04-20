using Playspot.Application.DTOs.Comments;

namespace Playspot.Application.Interfaces;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetCommentsForEventAsync(int eventId);
    Task<CommentResponseDto> AddCommentAsync(int eventId, int userId, CreateCommentDto dto);
}
