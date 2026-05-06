using System.ComponentModel.DataAnnotations;

namespace BeautySalonAPI.DTOs.Auth;

public record LoginDto(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
