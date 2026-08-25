using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class CloudBackupStateConfiguration : IEntityTypeConfiguration<CloudBackupState>
{
    public void Configure(EntityTypeBuilder<CloudBackupState> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InstallId).IsRequired();
        builder.HasIndex(e => e.InstallId).IsUnique();

        builder.Property(e => e.LastObjectKey).HasMaxLength(400);
        builder.Property(e => e.LastError).HasMaxLength(2000);
    }
}
