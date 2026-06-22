using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.IntegrationEvents;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.API.Controllers;
using OrderService.Infrastructure.Persistence;
using Xunit;

namespace OrderService.UnitTests;

public class OrdersControllerTests
{
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<ILogger<OrdersController>> _loggerMock = new();
    private readonly ApplicationDbContext _context;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _controller = new OrdersController(_context, _eventBusMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ConfirmOrder_ValidRequest_ShouldPublishEventAndReturn200()
    {
        var request = new ConfirmOrderRequest(Guid.NewGuid(), "CS101", 85m);

        var result = await _controller.ConfirmOrder(request);

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        _eventBusMock.Verify(b => b.PublishAsync(
            It.Is<OrderConfirmedIntegrationEvent>(e =>
                e.StudentId == request.StudentId &&
                e.CourseCode == request.CourseCode &&
                e.Score == request.Score)),
            Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task ConfirmOrder_InvalidScore_ShouldReturn400(decimal badScore)
    {
        var request = new ConfirmOrderRequest(Guid.NewGuid(), "CS101", badScore);

        var result = await _controller.ConfirmOrder(request);

        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.StatusCode.Should().Be(400);

        _eventBusMock.Verify(b => b.PublishAsync(It.IsAny<OrderConfirmedIntegrationEvent>()), Times.Never);
    }
}
