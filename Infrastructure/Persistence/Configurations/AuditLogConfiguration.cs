using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Action)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Summary)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.EntityType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasMany(e => e.Details)
            .WithOne(d => d.AuditLog)
            .HasForeignKey(d => d.AuditLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.Action, e.CreatedAt });
    }
}
