using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Grades.Commands;

public class CreateGradeCommandHandler(IGradeRepository repository) : IRequestHandler<CreateGradeCommand, Guid>
{
    public async Task<Guid> Handle(CreateGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = GradeRecord.Create(
            request.StudentId,
            request.CourseId,
            request.CourseName,
            request.Score,
            request.RecordedBy
        );

        await repository.AddAsync(grade, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return grade.Id;
    }
}
