using GradeService.Domain;

namespace GradeService.Application;

public interface IGradeRepository
{
    Task<Grade?> GetByStudentAndCourseAsync(Guid studentId, string courseCode, CancellationToken ct = default);
    Task AddAsync(Grade grade, CancellationToken ct = default);
    Task UpdateAsync(Grade grade, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
