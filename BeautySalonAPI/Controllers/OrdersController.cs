using BeautySalonAPI.Data;
using BeautySalonAPI.DTOs.Orders;
using BeautySalonAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonAPI.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create(CreateOrderDto dto)
    {
        var service = await db.Services.FindAsync(dto.ServiceId);
        if (service is null) return BadRequest("Service not found.");

        var order = new Order
        {
            ClientName = dto.ClientName,
            Phone      = dto.Phone,
            ServiceId  = dto.ServiceId,
            Date       = dto.Date,
            Time       = dto.Time,
            Notes      = dto.Notes ?? string.Empty,
            Price = dto.Price ?? service.Price,
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        return Ok(new { message = "Booking received! We will confirm shortly.", orderId = order.Id });
    }
}
