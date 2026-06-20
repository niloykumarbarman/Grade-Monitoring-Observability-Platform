namespace OrderService.Domain.Enums;

public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum AlertStatus
{
    Firing,
    Acknowledged,
    Resolved,
    Silenced
}
