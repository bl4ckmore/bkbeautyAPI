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
            Total: orders.Count,
            Pending: orders.Count(o => o.Status == OrderStatus.Pending),
            Confirmed: orders.Count(o => o.Status == OrderStatus.Confirmed),
            Completed: orders.Count(o => o.Status == OrderStatus.Completed),
            Cancelled: orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue: orders.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Price)
        ));
    }

    // ── Orders ─────────────────────────────────────────────
    // ids-only endpoint for notification polling — very lightweight
    [HttpGet("orders/ids")]
    public async Task<IActionResult> GetOrderIds()
    {
        var ids = await db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => o.Id)
            .ToListAsync();
        return Ok(ids);
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders([FromQuery] string? status)
    {
        var query = db.Orders
            .Include(o => o.Service)
            .Include(o => o.Stylist)   // User (client) join removed — rarely needed in list
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<OrderStatus>(status, true, out var parsed))
            query = query.Where(o => o.Status == parsed);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(ToDto));
    }

    [HttpPatch("orders/{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var order = await db.Orders
            .Include(o => o.Service)
            .Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        order.Status = dto.Status;
        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    [HttpPatch("orders/{id:int}/assign")]
    public async Task<IActionResult> AssignStylist(int id, AssignStylistDto dto)
    {
        var order = await db.Orders
            .Include(o => o.Service)
            .Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        if (dto.StylistId is not null)
        {
            var stylist = await db.Users.FindAsync(dto.StylistId);
            if (stylist is null || stylist.Role != "stylist")
                return BadRequest("Invalid stylist.");
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

    [HttpPatch("orders/{id:int}/schedule")]
    public async Task<IActionResult> UpdateSchedule(int id, UpdateScheduleDto dto)
    {
        var order = await db.Orders
            .Include(o => o.Service)
            .Include(o => o.Stylist)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        order.Date = dto.Date;
        order.Time = dto.Time;
        await db.SaveChangesAsync();
        return Ok(ToDto(order));
    }

    // ── Users ──────────────────────────────────────────────
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users
            .Include(u => u.Orders)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
        return Ok(users.Select(u =>
            new AdminUserDto(u.Id, u.Name, u.Email, u.Role, u.CreatedAt, u.Orders.Count)));
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
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "stylist",
        };

        db.Users.Add(stylist);
        await db.SaveChangesAsync();
        return Ok(new UserDto(stylist.Id, stylist.Name, stylist.Email, stylist.Role));
    }

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

    [HttpDelete("stylists/{id:int}")]
    public async Task<IActionResult> DeleteStylist(int id)
    {
        var stylist = await db.Users.FindAsync(id);
        if (stylist is null || stylist.Role != "stylist") return NotFound();
        db.Users.Remove(stylist);
        await db.SaveChangesAsync();
        return NoContent();
    }


    /// <summary>
    /// Returns per-stylist performance stats, optionally filtered by date range.
    /// GET /api/admin/reports/stylists?from=2024-01-01&to=2024-12-31
    /// </summary>
    [HttpGet("reports/stylists")]
    public async Task<IActionResult> GetStylistReport(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var query = db.Orders
            .Include(o => o.Service)
            .Include(o => o.Stylist)
            .AsQueryable();

        if (from.HasValue) query = query.Where(o => o.Date >= from.Value);
        if (to.HasValue) query = query.Where(o => o.Date <= to.Value);

        var orders = await query.ToListAsync();

        // Group by stylist (null = unassigned)
        var grouped = orders
            .GroupBy(o => o.StylistId)
            .Select(g =>
            {
                var stylistName = g.FirstOrDefault(o => o.Stylist != null)?.Stylist?.Name ?? "Unassigned";
                var completed = g.Where(o => o.Status == OrderStatus.Completed).ToList();

                return new StylistReportDto(
                    StylistId: g.Key,
                    StylistName: stylistName,
                    Total: g.Count(),
                    Completed: g.Count(o => o.Status == OrderStatus.Completed),
                    Confirmed: g.Count(o => o.Status == OrderStatus.Confirmed),
                    Pending: g.Count(o => o.Status == OrderStatus.Pending),
                    Cancelled: g.Count(o => o.Status == OrderStatus.Cancelled),
                    Revenue: completed.Sum(o => o.Price),
                    UniqueClients: g.Select(o => o.ClientName.Trim().ToLower()).Distinct().Count(),
                    TopService: g.GroupBy(o => o.Service?.Name ?? "Unknown")
                                      .OrderByDescending(x => x.Count())
                                      .Select(x => x.Key)
                                      .FirstOrDefault() ?? "—"
                );
            })
            .OrderByDescending(r => r.Revenue)
            .ToList();

        return Ok(grouped);
    }

    /// <summary>
    /// Returns month-by-month booking and revenue stats.
    /// GET /api/admin/reports/monthly?from=2024-01-01&to=2024-12-31
    /// </summary>
    [HttpGet("reports/monthly")]
    public async Task<IActionResult> GetMonthlyReport(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var query = db.Orders.AsQueryable();
        if (from.HasValue) query = query.Where(o => o.Date >= from.Value);
        if (to.HasValue) query = query.Where(o => o.Date <= to.Value);

        var orders = await query.ToListAsync();

        var monthly = orders
            .GroupBy(o => new { o.Date.Year, o.Date.Month })
            .Select(g => new
            {
                Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                Total = g.Count(),
                Completed = g.Count(o => o.Status == OrderStatus.Completed),
                Revenue = g.Where(o => o.Status == OrderStatus.Completed).Sum(o => o.Price),
            })
            .OrderBy(m => m.Month)
            .ToList();

        return Ok(monthly);
    }

    [HttpPatch("users/{id:int}/role")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto dto)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (dto.Role != "admin" && dto.Role != "stylist")
            return BadRequest(new { error = "Role must be 'admin' or 'stylist'." });

        // Prevent removing the last admin
        if (user.Role == "admin" && dto.Role != "admin")
        {
            var adminCount = await db.Users.CountAsync(u => u.Role == "admin");
            if (adminCount <= 1)
                return BadRequest(new { error = "Cannot remove the last admin." });
        }

        user.Role = dto.Role;
        await db.SaveChangesAsync();
        return Ok(new UserDto(user.Id, user.Name, user.Email, user.Role));
    }

    // ── Helper ─────────────────────────────────────────────
    private static AdminOrderDto ToDto(Order o) => new(
        o.Id, o.ClientName, o.Phone, o.Service?.Name ?? "", o.ServiceId,
        o.Date, o.Time, o.Status, o.Price, o.Notes, o.CreatedAt,
        null, null,          // User name/email removed from list query
        o.StylistId, o.Stylist?.Name
    );
}