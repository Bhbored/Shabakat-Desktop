using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.LicensedUntil)
            .IsRequired();

        builder.Property(e => e.LicenseStamp)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(e => e.BusinessName)
            .HasMaxLength(200);

        builder.Property(e => e.LogoUrl)
            .HasMaxLength(500);
    }
}
