using BeautySalonAPI.Models;

namespace BeautySalonAPI.DTOs.Orders;

public record UpdateOrderDto(
    string? ClientName,
    DateOnly? Date,
    TimeOnly? Time,
    OrderStatus? Status,
    string? Notes
);
