using BeautySalonAPI.Data;
using BeautySalonAPI.DTOs.Services;
using BeautySalonAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonAPI.Controllers;

[ApiController]
[Route("api/services")]
public class ServicesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var query = db.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(s => s.Category == category);

        var services = await query
            .OrderBy(s => s.Category).ThenBy(s => s.Name)
            .Select(s => ToDto(s))
            .ToListAsync();

        return Ok(services);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var s = await db.Services.FindAsync(id);
        if (s is null) return NotFound();
        return Ok(ToDto(s));
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create(ServiceMutateDto dto)
    {
        var service = new Service
        {
            Category    = dto.Category,
            Name        = dto.Name,
            Description = dto.Description,
            Duration    = dto.Duration,
            Price       = dto.Price,
            Icon        = dto.Icon,
            Popular     = dto.Popular,
        };

        db.Services.Add(service);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, ToDto(service));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, ServiceMutateDto dto)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null) return NotFound();

        service.Category    = dto.Category;
        service.Name        = dto.Name;
        service.Description = dto.Description;
        service.Duration    = dto.Duration;
        service.Price       = dto.Price;
        service.Icon        = dto.Icon;
        service.Popular     = dto.Popular;

        await db.SaveChangesAsync();
        return Ok(ToDto(service));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await db.Services.FindAsync(id);
        if (service is null) return NotFound();

        db.Services.Remove(service);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static ServiceDto ToDto(Service s) =>
        new(s.Id, s.Category, s.Name, s.Description, s.Duration, s.Price, s.Icon, s.Popular);
}
