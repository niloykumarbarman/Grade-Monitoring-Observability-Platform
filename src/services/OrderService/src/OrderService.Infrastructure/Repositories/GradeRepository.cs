using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories;

public class GradeRepository(AppDbContext context) : IGradeRepository
{
    public async Task<GradeRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.GradeRecords.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<GradeRecord>> GetByStudentIdAsync(string studentId, CancellationToken cancellationToken = default)
        => await context.GradeRecords
            .Where(g => g.StudentId == studentId)
            .OrderByDescending(g => g.RecordedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(GradeRecord grade, CancellationToken cancellationToken = default)
        => await context.GradeRecords.AddAsync(grade, cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
