using MediatR;
using Playspot.Application.DTOs.Users;

namespace Playspot.Application.Features.Users.Queries;

public class GetUserProfileQuery : IRequest<UserProfileDto?>
{
    public int UserId { get; }

    public GetUserProfileQuery(int userId)
    {
        UserId = userId;
    }
}
