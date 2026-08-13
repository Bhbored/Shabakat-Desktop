using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class AuditLogDetailsConfiguration : IEntityTypeConfiguration<AuditLogDetails>
{
    public void Configure(EntityTypeBuilder<AuditLogDetails> builder)
    {
        builder.ToTable("AuditLogDetails");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Label)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(e => e.AuditLogId);
    }
}
