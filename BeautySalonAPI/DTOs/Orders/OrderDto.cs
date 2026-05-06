using BeautySalonAPI.Models;

namespace BeautySalonAPI.DTOs.Orders;

public record OrderDto(
    int Id,
    string ClientName,
    string Phone,
    string Service,
    int ServiceId,
    DateOnly Date,
    TimeOnly Time,
    OrderStatus Status,
    decimal Price,
    string Notes,
    DateTime CreatedAt
);
