using Playspot.Application.DTOs.Events;

namespace Playspot.Application.Interfaces;

public interface IEventService
{
    Task<List<EventResponseDto>> GetAllAsync();
    Task<List<EventResponseDto>> GetFilteredAsync(EventFilterDto filters);
    Task<EventResponseDto?> GetByIdAsync(int id);
    Task<EventResponseDto> CreateAsync(CreateEventDto dto, int organizerId);
    Task<EventResponseDto?> UpdateAsync(int id, UpdateEventDto dto, int requestingUserId);
    Task<bool> DeleteAsync(int id, int requestingUserId);
    Task<List<EventResponseDto>> GetMyHostingAsync(int userId);
    Task<List<EventResponseDto>> GetMyJoinedAsync(int userId);
    Task<List<EventResponseDto>> GetMyPastAsync(int userId);
    Task<List<EventResponseDto>> GetSimilarAsync(int eventId);
}