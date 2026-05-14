using BeautySalonAPI.Data;
using BeautySalonAPI.DTOs.Admin;
using BeautySalonAPI.DTOs.Auth;
using BeautySalonAPI.DTOs.Orders;
using BeautySalonAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonAPI.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    // ── Stats ──────────────────────────────────────────────
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var orders = await db.Orders.ToListAsync();
        return Ok(new AdminStatsDto(
            Total:        orders.Count,
            Pending:      orders.Count(o => o.Status == OrderStatus.Pending),
            Confirmed:    orders.Count(o => o.Status == OrderStatus.Confirmed),
            Completed:    orders.Count(o => o.Status == OrderStatus.Completed),
            Cancelled:    orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue: orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Price)
        ));
    }

    // ── Orders ─────────────────────────────────────────────
    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] string? status)
    {
        var query = db.Orders
            .Include(o => o.Service)
            .Include(o => o.User)
            .Include(o => o.Stylist)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsed))
            query = query.Where(o => o.Status == parsed);

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders.Select(ToDto));
    }

    [HttpPatch("orders/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var order = await db.Orders.Include(o => o.Service).Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        order.Status = dto.Status;
        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    [HttpPatch("orders/{id:int}/assign")]
    public async Task<IActionResult> AssignStylist(int id, AssignStylistDto dto)
    {
        var order = await db.Orders.Include(o => o.Service).Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        if (dto.StylistId is not null)
        {
            var stylist = await db.Users.FindAsync(dto.StylistId);
            if (stylist is null || stylist.Role != "stylist") return BadRequest("Invalid stylist.");
        }

        order.StylistId = dto.StylistId;
        await db.SaveChangesAsync();

        if (order.StylistId is not null)
            await db.Entry(order).Reference(o => o.Stylist).LoadAsync();

        return Ok(ToDto(order));
    }

    [HttpDelete("orders/{id:int}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ── Users ──────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users.Include(u => u.Orders).OrderBy(u => u.CreatedAt).ToListAsync();
        return Ok(users.Select(u => new AdminUserDto(u.Id, u.Name, u.Email, u.Role, u.CreatedAt, u.Orders.Count)));
    }

    // ── Stylists ───────────────────────────────────────────
    [HttpGet("stylists")]
    public async Task<IActionResult> GetStylists()
    {
        var stylists = await db.Users
            .Where(u => u.Role == "stylist")
            .OrderBy(u => u.Name)
            .ToListAsync();

        return Ok(stylists.Select(u => new UserDto(u.Id, u.Name, u.Email, u.Role)));
    }

    [HttpPost("stylists")]
    public async Task<IActionResult> CreateStylist(CreateStylistDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict(new { error = "Email already in use." });

        var stylist = new User
        {
            Name         = dto.Name,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = "stylist",
        };

        db.Users.Add(stylist);
        await db.SaveChangesAsync();
        return Ok(new UserDto(stylist.Id, stylist.Name, stylist.Email, stylist.Role));
    }

    [HttpDelete("stylists/{id:int}")]
    public async Task<IActionResult> DeleteStylist(int id)
    {
        var stylist = await db.Users.FindAsync(id);
        if (stylist is null || stylist.Role != "stylist") return NotFound();
        db.Users.Remove(stylist);
        await db.SaveChangesAsync();
        return NoContent();
    }



    // PATCH /api/admin/orders/{id}/client  — update clientName + phone
    [HttpPatch("orders/{id:int}/client")]
    public async Task<IActionResult> UpdateClient(int id, UpdateClientDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ClientName))
            return BadRequest(new { error = "Client name is required." });

        var order = await db.Orders
            .Include(o => o.Service)
            .Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null) return NotFound();

        order.ClientName = dto.ClientName.Trim();
        order.Phone = dto.Phone?.Trim() ?? string.Empty;

        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    // PUT /api/admin/stylists/{id}  — update name / email / password
    [HttpPut("stylists/{id:int}")]
    public async Task<IActionResult> UpdateStylist(int id, UpdateStylistDto dto)
    {
        var stylist = await db.Users.FindAsync(id);
        if (stylist is null || stylist.Role != "stylist") return NotFound();

        if (await db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id))
            return Conflict(new { error = "Email already in use." });

        stylist.Name = dto.Name.Trim();
        stylist.Email = dto.Email.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Password))
            stylist.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        await db.SaveChangesAsync();
        return Ok(new UserDto(stylist.Id, stylist.Name, stylist.Email, stylist.Role));
    }

    [HttpPatch("orders/{id:int}/schedule")]
    public async Task<IActionResult> UpdateSchedule(int id, UpdateScheduleDto dto)
    {
        var order = await db.Orders.Include(o => o.Service).Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        order.Date = dto.Date;
        order.Time = dto.Time;
        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }


    // ── Helper ─────────────────────────────────────────────
    private static AdminOrderDto ToDto(Order o) => new(
        o.Id, o.ClientName, o.Phone, o.Service.Name, o.ServiceId,
        o.Date, o.Time, o.Status, o.Price, o.Notes, o.CreatedAt,
        o.User?.Name, o.User?.Email,
        o.StylistId, o.Stylist?.Name
    );
}
