using Playspot.Application.DTOs.JoinRequests;

namespace Playspot.Application.Interfaces;

public interface IJoinRequestService
{
    Task<JoinRequestResponseDto> RequestJoinAsync(int eventId, int userId);
    Task<JoinRequestResponseDto> UpdateStatusAsync(int requestId, string status, int organiserId);
    Task<List<JoinRequestResponseDto>> GetRequestsForEventAsync(int eventId, int organiserId);
}