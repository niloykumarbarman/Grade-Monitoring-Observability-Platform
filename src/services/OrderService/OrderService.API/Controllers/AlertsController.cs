using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.API.Models;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Infrastructure.Persistence;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(ApplicationDbContext context, ILogger<AlertsController> logger)
    {
        _context = context;
        _logger = logger;
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
            .CountAsync(a => a.Status == AlertStatus.Firing);
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
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAlerts), new { id = alert.Id }, alert);
    }
}

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(ApplicationDbContext context, ILogger<WebhooksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // POST /api/webhooks/alerts  (called by Alertmanager)
    [HttpPost("alerts")]
    public async Task<IActionResult> ReceiveAlertmanagerWebhook([FromBody] AlertmanagerWebhookPayload payload)
    {
        _logger.LogInformation(
            "Received {Count} alert(s) from Alertmanager with status {Status}",
            payload.Alerts.Count, payload.Status);

        foreach (var item in payload.Alerts)
        {
            var alertName = item.Labels.GetValueOrDefault("alertname", "UnknownAlert");
            var severityLabel = item.Labels.GetValueOrDefault("severity", "info");

            var alert = new Alert
            {
                Id = Guid.NewGuid(),
                Title = alertName,
                Description = item.Annotations.GetValueOrDefault("summary"),
                Severity = ParseSeverity(severityLabel),
                Status = item.Status == "resolved" ? AlertStatus.Resolved : AlertStatus.Firing,
                Source = "alertmanager",
                Labels = System.Text.Json.JsonSerializer.Serialize(item.Labels),
                FiredAt = item.StartsAt,
                ResolvedAt = item.EndsAt
            };

            _context.Alerts.Add(alert);
        }

        await _context.SaveChangesAsync();
        return Ok(new { received = payload.Alerts.Count });
    }

    private static AlertSeverity ParseSeverity(string severity) => severity.ToLowerInvariant() switch
    {
        "critical" => AlertSeverity.Critical,
        "warning" => AlertSeverity.Warning,
        _ => AlertSeverity.Info
    };
}
