using System.ComponentModel.DataAnnotations;

namespace BeautySalonAPI.DTOs.Admin;

public record CreateStylistDto(
    [Required, MinLength(2)] string Name,
    [Required, EmailAddress]  string Email,
    [Required, MinLength(6)]  string Password
);
