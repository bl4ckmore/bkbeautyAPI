using BeautySalonAPI.Data;
using BeautySalonAPI.DTOs.Auth;
using BeautySalonAPI.Models;
using BeautySalonAPI.Services.Token;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonAPI.Services.Auth;

public class AuthService(AppDbContext db, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user) =>
        new(tokenService.Generate(user), new UserDto(user.Id, user.Name, user.Email, user.Role));
}
