using Playspot.Application.DTOs.Ratings;

namespace Playspot.Application.Interfaces;

public interface IRatingService
{
    Task<RatingResponseDto?> RateEventAsync(int eventId, int userId, CreateRatingDto dto);
    Task<List<RatingResponseDto>> GetRatingsForEventAsync(int eventId);
    Task<double> GetAverageRatingAsync(int eventId);
}
