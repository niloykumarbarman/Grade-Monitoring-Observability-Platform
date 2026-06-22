using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.IntegrationEvents;
using Microsoft.AspNetCore.Mvc;
using OrderService.Infrastructure.Persistence;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ApplicationDbContext context, IEventBus eventBus, ILogger<OrdersController> logger)
    {
        _context = context;
        _eventBus = eventBus;
        _logger = logger;
    }

    // POST /api/orders/confirm
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmOrder([FromBody] ConfirmOrderRequest request)
    {
        if (request.Score < 0 || request.Score > 100)
            return BadRequest(new { error = "Score must be between 0 and 100." });

        var orderId = Guid.NewGuid();

        _logger.LogInformation(
            "Confirming order {OrderId} for student {StudentId}, course {CourseCode}, score {Score}",
            orderId, request.StudentId, request.CourseCode, request.Score);

        var integrationEvent = new OrderConfirmedIntegrationEvent(
            orderId,
            request.StudentId,
            request.CourseCode,
            request.Score);

        await _eventBus.PublishAsync(integrationEvent);

        _logger.LogInformation("Published OrderConfirmedIntegrationEvent for OrderId={OrderId}", orderId);

        return Ok(new
        {
            orderId,
            studentId = request.StudentId,
            courseCode = request.CourseCode,
            score = request.Score,
            message = "Order confirmed and grade update event published."
        });
    }
}

public record ConfirmOrderRequest(Guid StudentId, string CourseCode, decimal Score);
