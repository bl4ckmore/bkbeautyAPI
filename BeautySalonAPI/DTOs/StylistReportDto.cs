namespace BeautySalonAPI.DTOs.Admin;

public record StylistReportDto(
    int? StylistId,
    string StylistName,
    int Total,
    int Completed,
    int Confirmed,
    int Pending,
    int Cancelled,
    decimal Revenue,
    int UniqueClients,
    string TopService
);