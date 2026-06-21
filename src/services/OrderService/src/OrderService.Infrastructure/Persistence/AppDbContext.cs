using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GradeRecord> GradeRecords => Set<GradeRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GradeRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StudentId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CourseId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CourseName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Score).HasPrecision(5, 2);
            entity.Property(e => e.Grade).IsRequired().HasMaxLength(5);
            entity.Property(e => e.RecordedBy).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.CourseId);
        });
    }
}
