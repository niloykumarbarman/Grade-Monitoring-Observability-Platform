using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AlertsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/alerts
    [HttpGet]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = await _context.Alerts
            .OrderByDescending(a => a.FiredAt)
            .Take(50)
            .ToListAsync();

        return Ok(alerts);
    }

    // GET /api/alerts/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var total = await _context.Alerts.CountAsync();
        var firing = await _context.Alerts
            .CountAsync(a => a.Status == OrderService.Domain.Enums.AlertStatus.Firing);

        return Ok(new
        {
            totalAlerts = total,
            firingAlerts = firing,
            systemHealth = firing > 0 ? "WARNING" : "OK"
        });
    }

    // POST /api/alerts
    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] Alert alert)
    {
        alert.Id = Guid.NewGuid();
        alert.FiredAt = DateTimeOffset.UtcNow;
        alert.CreatedAt = DateTimeOffset.UtcNow;
        alert.UpdatedAt = DateTimeOffset.UtcNow;

        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAlerts), new { id = alert.Id }, alert);
    }
}
