using System.Security.Claims;
using BeautySalonAPI.Data;
using BeautySalonAPI.DTOs.Admin;
using BeautySalonAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonAPI.Controllers;

[ApiController]
[Route("api/stylist")]
[Authorize(Roles = "stylist")]
public class StylistController(AppDbContext db) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> GetMyOrders([FromQuery] string? status)
    {
        var stylistId = GetUserId();
        var query = db.Orders
            .Include(o => o.Service)
            .Where(o => o.StylistId == stylistId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            query = query.Where(o => o.Status == parsed);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(o => new AdminOrderDto(
            o.Id, o.ClientName, o.Phone, o.Service.Name, o.ServiceId,
            o.Date, o.Time, o.Status, o.Price, o.Notes, o.CreatedAt,
            null, null, o.StylistId, null
        )));
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetMyStats()
    {
        var stylistId = GetUserId();
        var orders = await db.Orders.Where(o => o.StylistId == stylistId).ToListAsync();
        return Ok(new
        {
            total     = orders.Count,
            pending   = orders.Count(o => o.Status == OrderStatus.Pending),
            confirmed = orders.Count(o => o.Status == OrderStatus.Confirmed),
            completed = orders.Count(o => o.Status == OrderStatus.Completed),
            cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled),
        });
    }

    [HttpPatch("orders/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var stylistId = GetUserId();
        var order = await db.Orders.Include(o => o.Service)
            .FirstOrDefaultAsync(o => o.Id == id && o.StylistId == stylistId);
        if (order is null) return NotFound();

        order.Status = dto.Status;
        await db.SaveChangesAsync();

        return Ok(new AdminOrderDto(
            order.Id, order.ClientName, order.Phone, order.Service.Name, order.ServiceId,
            order.Date, order.Time, order.Status, order.Price, order.Notes, order.CreatedAt,
            null, null, order.StylistId, null
        ));
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
