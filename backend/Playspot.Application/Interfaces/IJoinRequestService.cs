using Playspot.Application.DTOs.JoinRequests;

namespace Playspot.Application.Interfaces;

public interface IJoinRequestService
{
    Task<JoinRequestResponseDto?> RequestToJoinAsync(int eventId, int userId);
    Task<List<JoinRequestResponseDto>> GetRequestsForEventAsync(int eventId, int organizerId);
    Task<bool> UpdateStatusAsync(int requestId, string status, int organizerId);
    Task<bool> WithdrawAsync(int eventId, int userId);
}