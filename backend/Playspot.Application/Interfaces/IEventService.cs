using Playspot.Application.DTOs.Events;

namespace Playspot.Application.Interfaces;

public interface IEventService
{
    Task<List<EventResponseDto>> GetAllAsync(string? sport, string? location);
    Task<EventResponseDto?> GetByIdAsync(int id);
    Task<EventResponseDto> CreateAsync(CreateEventDto dto, int organiserId);
    Task DeleteAsync(int id, int requestingUserId);
}