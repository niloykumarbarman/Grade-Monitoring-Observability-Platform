// Persistence/Configurations/AuditLogConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs");
        b.HasKey(a => a.Id);

        b.Property(a => a.Action).HasMaxLength(50).IsRequired();
        b.Property(a => a.OldValues).HasColumnType("jsonb");
        b.Property(a => a.NewValues).HasColumnType("jsonb");

        b.HasIndex(a => a.UserId);
        b.HasIndex(a => a.CreatedAt);
    }
}
