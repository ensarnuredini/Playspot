using Playspot.Application.DTOs.Events;

namespace Playspot.Application.Interfaces;

public interface ISavedEventService
{
    Task<bool> SaveAsync(int eventId, int userId);
    Task<bool> UnsaveAsync(int eventId, int userId);
    Task<List<EventResponseDto>> GetSavedByUserAsync(int userId);
    Task<bool> IsEventSavedAsync(int eventId, int userId);
}
