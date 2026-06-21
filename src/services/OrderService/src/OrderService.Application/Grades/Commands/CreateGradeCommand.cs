using MediatR;

namespace OrderService.Application.Grades.Commands;

public record CreateGradeCommand(
    string StudentId,
    string CourseId,
    string CourseName,
    decimal Score,
    string RecordedBy
) : IRequest<Guid>;
