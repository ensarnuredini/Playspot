using Playspot.Domain.Entities;

namespace Playspot.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
