namespace BeautySalonAPI.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    UserDto User
);

public record UserDto(
    int Id,
    string Name,
    string Email,
    string Role
);
