namespace BeautySalonAPI.Models;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Completed
}

public class Order
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public int? StylistId { get; set; }
    public User? Stylist { get; set; }
}
