namespace BeautySalonAPI.Models;

public class Service
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Icon { get; set; } = string.Empty;
    public bool Popular { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}
