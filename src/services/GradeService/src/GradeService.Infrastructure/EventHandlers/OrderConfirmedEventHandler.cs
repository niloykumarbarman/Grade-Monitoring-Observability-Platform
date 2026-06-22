using BuildingBlocks.EventBus.Abstractions;
using BuildingBlocks.EventBus.IntegrationEvents;
using GradeService.Application;
using GradeService.Domain;
using Microsoft.Extensions.Logging;

namespace GradeService.Infrastructure.EventHandlers;

public class OrderConfirmedEventHandler : IIntegrationEventHandler<OrderConfirmedIntegrationEvent>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly ILogger<OrderConfirmedEventHandler> _logger;

    public OrderConfirmedEventHandler(IGradeRepository gradeRepository, ILogger<OrderConfirmedEventHandler> logger)
    {
        _gradeRepository = gradeRepository;
        _logger = logger;
    }

    public async Task Handle(OrderConfirmedIntegrationEvent @eventData)
    {
        _logger.LogInformation(
            "Processing OrderConfirmed: OrderId={OrderId}, StudentId={StudentId}, Course={CourseCode}, Score={Score}",
            @eventData.OrderId, @eventData.StudentId, @eventData.CourseCode, @eventData.Score);

        var existing = await _gradeRepository.GetByStudentAndCourseAsync(
            @eventData.StudentId, @eventData.CourseCode);

        if (existing is null)
        {
            var grade = Grade.Create(
                @eventData.OrderId,
                @eventData.StudentId,
                @eventData.CourseCode,
                @eventData.Score);

            await _gradeRepository.AddAsync(grade);
            _logger.LogInformation("Grade created: {GradeId} → {LetterGrade}", grade.Id, grade.LetterGrade);
        }
        else
        {
            existing.UpdateScore(@eventData.Score);
            await _gradeRepository.UpdateAsync(existing);
            _logger.LogInformation("Grade updated for StudentId={StudentId}, Course={CourseCode}", 
                @eventData.StudentId, @eventData.CourseCode);
        }

        await _gradeRepository.SaveChangesAsync();
    }
}
