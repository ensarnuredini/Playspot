using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Ratings;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;
using Playspot.Infrastructure.Data;

namespace Playspot.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly AppDbContext _db;

    public RatingService(AppDbContext db) => _db = db;

    public async Task<RatingResponseDto?> RateEventAsync(int eventId, int userId, CreateRatingDto dto)
    {
        // Check if already rated
        var exists = await _db.EventRatings
            .AnyAsync(r => r.EventId == eventId && r.UserId == userId);

        if (exists) return null;

        var rating = new EventRating
        {
            EventId = eventId,
            UserId = userId,
            Score = Math.Clamp(dto.Score, 1, 5),
            Comment = dto.Comment
        };

        _db.EventRatings.Add(rating);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);

        return new RatingResponseDto
        {
            Id = rating.Id,
            EventId = rating.EventId,
            UserId = rating.UserId,
            Username = user?.Username ?? "Unknown",
            Score = rating.Score,
            Comment = rating.Comment,
            CreatedAt = rating.CreatedAt
        };
    }

    public async Task<List<RatingResponseDto>> GetRatingsForEventAsync(int eventId)
    {
        return await _db.EventRatings
            .Where(r => r.EventId == eventId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new RatingResponseDto
            {
                Id = r.Id,
                EventId = r.EventId,
                UserId = r.UserId,
                Username = r.User.Username,
                Score = r.Score,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingAsync(int eventId)
    {
        var ratings = await _db.EventRatings
            .Where(r => r.EventId == eventId)
            .ToListAsync();

        return ratings.Count == 0 ? 0 : ratings.Average(r => r.Score);
    }
}
