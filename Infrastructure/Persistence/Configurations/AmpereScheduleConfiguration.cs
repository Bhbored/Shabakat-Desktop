using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class AmpereScheduleConfiguration : IEntityTypeConfiguration<AmpereSchedule>
{
    public void Configure(EntityTypeBuilder<AmpereSchedule> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.HoursPerDay)
            .IsRequired();

        builder.Property(e => e.PricePerAmp)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.ResidentialPricePerAmp)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.CommercialPricePerAmp)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.IndustrialPricePerAmp)
            .HasColumnType("decimal(18,4)");

        builder.HasIndex(e => e.HoursPerDay)
            .IsUnique();
    }
}
