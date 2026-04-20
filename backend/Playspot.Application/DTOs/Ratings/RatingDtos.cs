namespace Playspot.Application.DTOs.Ratings;

public class CreateRatingDto
{
    public int Score { get; set; } // 1-5
    public string? Comment { get; set; }
}

public class RatingResponseDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
