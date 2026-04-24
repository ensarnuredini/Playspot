using MediatR;
using Microsoft.EntityFrameworkCore;
using Playspot.Application.DTOs.Auth;
using Playspot.Application.Interfaces;
using Playspot.Domain.Entities;

namespace Playspot.Application.Features.Auth.Commands;

// ── Register ──
public record RegisterCommand(RegisterDto Dto) : IRequest<AuthResponseDto?>;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponseDto?>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;

    public RegisterHandler(IAppDbContext db, IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto?> Handle(RegisterCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        if (await _db.Users.AnyAsync(u => u.Email == dto.Email, ct))
            return null;

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            City = dto.City
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponseDto
        {
            Token = _jwt.GenerateToken(user),
            Username = user.Username,
            UserId = user.Id,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }
}

// ── Login ──
public record LoginCommand(LoginDto Dto) : IRequest<AuthResponseDto?>;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResponseDto?>
{
    private readonly IAppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;

    public LoginHandler(IAppDbContext db, IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto?> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == request.Dto.Email, ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Dto.Password, user.PasswordHash))
            return null;

        return new AuthResponseDto
        {
            Token = _jwt.GenerateToken(user),
            Username = user.Username,
            UserId = user.Id,
            ProfileImageUrl = user.ProfileImageUrl
        };
    }
}
