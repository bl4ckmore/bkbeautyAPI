namespace BeautySalonAPI.DTOs.Services;

public record ServiceDto(
    int Id,
    string Category,
    string Name,
    string Description,
    string Duration,
    decimal Price,
    string Icon,
    bool Popular
);
