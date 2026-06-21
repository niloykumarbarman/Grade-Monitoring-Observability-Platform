namespace OrderService.API.Models;

public class AlertmanagerWebhookPayload
{
    public string Status { get; set; } = default!;
    public List<AlertmanagerAlert> Alerts { get; set; } = new();
    public Dictionary<string, string> CommonLabels { get; set; } = new();
    public Dictionary<string, string> CommonAnnotations { get; set; } = new();
}

public class AlertmanagerAlert
{
    public string Status { get; set; } = default!;
    public Dictionary<string, string> Labels { get; set; } = new();
    public Dictionary<string, string> Annotations { get; set; } = new();
    public DateTimeOffset StartsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
}
