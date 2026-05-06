using System.ComponentModel.DataAnnotations;

namespace BeautySalonAPI.DTOs.Orders;

public record CreateOrderDto(
    [Required, MinLength(2)] string ClientName,
    [Required] string Phone,
    [Required] int ServiceId,
    [Required] DateOnly Date,
    [Required] TimeOnly Time,
    string Notes
);
