namespace BeautySalonAPI.DTOs.Admin
{
    public record UpdateStylistDto(string Name, string Email, string? Password);
}


public record CreateOrderDto(
    string ClientName, string Phone,
    int ServiceId,
    DateOnly Date, TimeOnly Time,
    string? Notes,
    decimal? Price   // ← add this
);