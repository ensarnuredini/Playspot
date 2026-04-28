using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Users;
using Playspot.Application.Interfaces;

namespace Playspot.Application.Features.Users.Queries;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto?>
{
    private readonly IAppDbContext _context;

    public GetUserProfileHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto?> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.OrganizedEvents)
            .Include(u => u.JoinRequests)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            City = user.City,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            EventsCreatedCount = user.OrganizedEvents.Count,
            EventsJoinedCount = user.JoinRequests.Count(jr => jr.Status == "Approved") 
        };
    }
}
