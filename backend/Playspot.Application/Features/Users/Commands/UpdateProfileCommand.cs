using MediatR;
using Playspot.Application.DTOs.Users;

namespace Playspot.Application.Features.Users.Commands;

public class UpdateProfileCommand : IRequest<UserProfileDto?>
{
    public int UserId { get; }
    public UpdateProfileDto Dto { get; }

    public UpdateProfileCommand(int userId, UpdateProfileDto dto)
    {
        UserId = userId;
        Dto = dto;
    }
}
