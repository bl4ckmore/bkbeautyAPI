using System.ComponentModel.DataAnnotations;

namespace BeautySalonAPI.DTOs.Services;

public record ServiceMutateDto(
    [Required] string Category,
    [Required, MinLength(2)] string Name,
    [Required] string Description,
    [Required] string Duration,
    [Required, Range(0.01, 100000)] decimal Price,
    [Required] string Icon,
    bool Popular
);
