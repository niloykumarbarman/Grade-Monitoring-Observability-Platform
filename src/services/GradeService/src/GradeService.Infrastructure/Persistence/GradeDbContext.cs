using GradeService.Domain;
using Microsoft.EntityFrameworkCore;

namespace GradeService.Infrastructure.Persistence;

public class GradeDbContext : DbContext
{
    public GradeDbContext(DbContextOptions<GradeDbContext> options) : base(options) { }

    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Grade>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.CourseCode).HasMaxLength(20).IsRequired();
            e.Property(g => g.LetterGrade).HasMaxLength(3).IsRequired();
            e.Property(g => g.Score).HasPrecision(5, 2);
            e.HasIndex(g => new { g.StudentId, g.CourseCode });
        });
    }
}
