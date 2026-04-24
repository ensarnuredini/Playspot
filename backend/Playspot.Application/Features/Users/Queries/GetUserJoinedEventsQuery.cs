using MediatR;
using Playspot.Application.DTOs.Events;

namespace Playspot.Application.Features.Users.Queries;

public class GetUserJoinedEventsQuery : IRequest<List<EventResponseDto>>
{
    public int UserId { get; }

    public GetUserJoinedEventsQuery(int userId)
    {
        UserId = userId;
    }
}
