namespace Playspot.Application.DTOs.Comments;

public class CreateCommentDto
{
    public string Text { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsHost { get; set; }
    public DateTime CreatedAt { get; set; }
}
