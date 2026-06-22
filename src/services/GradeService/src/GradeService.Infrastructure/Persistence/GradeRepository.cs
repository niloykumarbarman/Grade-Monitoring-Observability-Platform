using GradeService.Application;
using GradeService.Domain;
using Microsoft.EntityFrameworkCore;

namespace GradeService.Infrastructure.Persistence;

public class GradeRepository : IGradeRepository
{
    private readonly GradeDbContext _context;

    public GradeRepository(GradeDbContext context) => _context = context;

    public Task<Grade?> GetByStudentAndCourseAsync(Guid studentId, string courseCode, CancellationToken ct = default)
        => _context.Grades.FirstOrDefaultAsync(g => g.StudentId == studentId && g.CourseCode == courseCode, ct);

    public async Task AddAsync(Grade grade, CancellationToken ct = default)
        => await _context.Grades.AddAsync(grade, ct);

    public Task UpdateAsync(Grade grade, CancellationToken ct = default)
    {
        _context.Grades.Update(grade);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
