using BeautySalonAPI.Models;

namespace BeautySalonAPI.DTOs.Admin;

public record AdminOrderDto(
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
    DateTime CreatedAt,
    string? UserName,
    string? UserEmail,
    int? StylistId,
    string? StylistName
);
