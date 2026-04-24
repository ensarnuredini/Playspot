using MediatR;
using Playspot.Application.DTOs.Events;

namespace Playspot.Application.Features.Users.Queries;

public class GetUserEventsQuery : IRequest<List<EventResponseDto>>
{
    public int UserId { get; }

    public GetUserEventsQuery(int userId)
    {
        UserId = userId;
    }
}
