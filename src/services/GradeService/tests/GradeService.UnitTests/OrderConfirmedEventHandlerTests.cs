using BuildingBlocks.EventBus.IntegrationEvents;
using FluentAssertions;
using GradeService.Application;
using GradeService.Domain;
using GradeService.Infrastructure.EventHandlers;
using Microsoft.Extensions.Logging;
using Moq;

namespace GradeService.UnitTests;

public class OrderConfirmedEventHandlerTests
{
    private readonly Mock<IGradeRepository> _repoMock = new();
    private readonly Mock<ILogger<OrderConfirmedEventHandler>> _loggerMock = new();
    private readonly OrderConfirmedEventHandler _handler;

    public OrderConfirmedEventHandlerTests()
    {
        _handler = new OrderConfirmedEventHandler(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_NewStudent_ShouldCreateGrade()
    {
        var evt = new OrderConfirmedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "CS101", 85m);

        _repoMock
            .Setup(r => r.GetByStudentAndCourseAsync(evt.StudentId, evt.CourseCode, default))
            .ReturnsAsync((Grade?)null);

        await _handler.Handle(evt);

        _repoMock.Verify(r => r.AddAsync(It.Is<Grade>(g =>
            g.StudentId == evt.StudentId &&
            g.CourseCode == evt.CourseCode &&
            g.Score == evt.Score), default), Times.Once);

        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Grade>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ExistingStudent_ShouldUpdateGrade()
    {
        var studentId = Guid.NewGuid();
        var existing = Grade.Create(Guid.NewGuid(), studentId, "CS101", 60m);
        var evt = new OrderConfirmedIntegrationEvent(
            Guid.NewGuid(), studentId, "CS101", 90m);

        _repoMock
            .Setup(r => r.GetByStudentAndCourseAsync(studentId, "CS101", default))
            .ReturnsAsync(existing);

        await _handler.Handle(evt);

        existing.Score.Should().Be(90m);
        existing.LetterGrade.Should().Be("A+");

        _repoMock.Verify(r => r.UpdateAsync(existing, default), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<Grade>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCallSaveChanges_Always()
    {
        var evt = new OrderConfirmedIntegrationEvent(
            Guid.NewGuid(), Guid.NewGuid(), "ENG301", 70m);

        _repoMock
            .Setup(r => r.GetByStudentAndCourseAsync(evt.StudentId, evt.CourseCode, default))
            .ReturnsAsync((Grade?)null);

        await _handler.Handle(evt);

        _repoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }
}
