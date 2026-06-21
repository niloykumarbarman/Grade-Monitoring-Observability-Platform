using Common.Domain;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class Alert : BaseEntity<Guid>
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public AlertSeverity Severity { get; set; } = AlertSeverity.Info;
    public AlertStatus Status { get; set; } = AlertStatus.Firing;
    public string? Source { get; set; }
    public string Labels { get; set; } = "{}"; // JSONB
    public DateTimeOffset FiredAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid? AcknowledgedBy { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
