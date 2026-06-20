// Persistence/Configurations/AlertConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> b)
    {
        b.ToTable("alerts");
        b.HasKey(a => a.Id);

        b.Property(a => a.Title).HasMaxLength(255).IsRequired();
        b.Property(a => a.Severity).HasConversion<string>().IsRequired();
        b.Property(a => a.Status).HasConversion<string>().IsRequired();
        b.Property(a => a.Labels).HasColumnType("jsonb");

        b.HasIndex(a => a.Status);
        b.HasIndex(a => a.Severity);
        b.HasIndex(a => a.FiredAt);
    }
}
