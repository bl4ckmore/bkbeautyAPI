namespace BeautySalonAPI.DTOs.Admin;

public record AdminStatsDto(
    int Total,
    int Pending,
    int Confirmed,
    int Completed,
    int Cancelled,
    decimal TotalRevenue
);
