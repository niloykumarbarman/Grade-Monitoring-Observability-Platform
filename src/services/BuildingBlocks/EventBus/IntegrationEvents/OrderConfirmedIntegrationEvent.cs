using BuildingBlocks.EventBus.Events;

namespace BuildingBlocks.EventBus.IntegrationEvents;

public class OrderConfirmedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; set; }
    public Guid StudentId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public decimal Score { get; set; }

    public OrderConfirmedIntegrationEvent(Guid orderId, Guid studentId, string courseCode, decimal score)
    {
        OrderId = orderId;
        StudentId = studentId;
        CourseCode = courseCode;
        Score = score;
    }
}
