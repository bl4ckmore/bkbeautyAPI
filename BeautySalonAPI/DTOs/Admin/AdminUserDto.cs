namespace BeautySalonAPI.DTOs.Admin;

public record AdminUserDto(
    int Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt,
    int OrderCount
);
