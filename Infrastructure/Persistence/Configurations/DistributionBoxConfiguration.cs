using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class DistributionBoxConfiguration : IEntityTypeConfiguration<DistributionBox>
{
    public void Configure(EntityTypeBuilder<DistributionBox> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.AreaId)
            .IsRequired();

        builder.Property(e => e.LocationNote)
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.HasOne(e => e.Area)
            .WithMany(a => a.DistributionBoxes)
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.AreaId);
        builder.HasIndex(e => e.Name);
    }
}
