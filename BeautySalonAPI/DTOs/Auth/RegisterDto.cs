using System.ComponentModel.DataAnnotations;

namespace BeautySalonAPI.DTOs.Auth;

public record RegisterDto(
    [Required, MinLength(2)] string Name,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);
