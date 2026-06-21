using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IGradeRepository
{
    Task<GradeRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GradeRecord>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default);
    Task AddAsync(GradeRecord grade, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
