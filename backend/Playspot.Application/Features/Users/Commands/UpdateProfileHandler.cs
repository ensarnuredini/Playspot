using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Users;
using Playspot.Application.Interfaces;

namespace Playspot.Application.Features.Users.Commands;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, UserProfileDto?>
{
    private readonly IAppDbContext _context;

    public UpdateProfileHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto?> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.OrganizedEvents)
            .Include(u => u.JoinRequests)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            return null;

        if (request.Dto.FirstName != null)
            user.FirstName = request.Dto.FirstName;
        
        if (request.Dto.LastName != null)
            user.LastName = request.Dto.LastName;

        if (request.Dto.City != null)
            user.City = request.Dto.City;

        if (request.Dto.Bio != null)
            user.Bio = request.Dto.Bio;
        
        if (request.Dto.ProfileImageUrl != null)
            user.ProfileImageUrl = request.Dto.ProfileImageUrl;

        await _context.SaveChangesAsync(cancellationToken);

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
            EventsJoinedCount = user.JoinRequests.Count(jr => jr.Status == "Accepted") 
        };
    }
}
