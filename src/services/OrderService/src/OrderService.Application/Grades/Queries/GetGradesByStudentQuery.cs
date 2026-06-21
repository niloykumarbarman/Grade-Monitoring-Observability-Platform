using MediatR;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Application.Grades.Queries;

public record GetGradesByStudentQuery(string StudentId) : IRequest<IEnumerable<GradeRecord>>;

public class GetGradesByStudentQueryHandler(IGradeRepository repository)
    : IRequestHandler<GetGradesByStudentQuery, IEnumerable<GradeRecord>>
{
    public async Task<IEnumerable<GradeRecord>> Handle(GetGradesByStudentQuery request, CancellationToken cancellationToken)
        => await repository.GetByStudentIdAsync(request.StudentId, cancellationToken);
}
