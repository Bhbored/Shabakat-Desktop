using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class AppPreferencesConfiguration : IEntityTypeConfiguration<AppPreferences>
{
    public void Configure(EntityTypeBuilder<AppPreferences> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PricePerKilowat).HasColumnType("decimal(18,4)");
        builder.Property(e => e.PricePerAmp).HasColumnType("decimal(18,4)");
        builder.Property(e => e.FixedCharge).HasColumnType("decimal(18,4)");
        builder.Property(e => e.TVA).HasColumnType("decimal(5,2)");

        builder.Property(e => e.ResidentialPricePerAmp).HasColumnType("decimal(18,4)");
        builder.Property(e => e.ResidentialPricePerKilowat).HasColumnType("decimal(18,4)");
        builder.Property(e => e.ResidentialFixedCharge).HasColumnType("decimal(18,4)");
        builder.Property(e => e.ResidentialTVA).HasColumnType("decimal(5,2)");

        builder.Property(e => e.CommercialPricePerAmp).HasColumnType("decimal(18,4)");
        builder.Property(e => e.CommercialPricePerKilowat).HasColumnType("decimal(18,4)");
        builder.Property(e => e.CommercialFixedCharge).HasColumnType("decimal(18,4)");
        builder.Property(e => e.CommercialTVA).HasColumnType("decimal(5,2)");

        builder.Property(e => e.IndustrialPricePerAmp).HasColumnType("decimal(18,4)");
        builder.Property(e => e.IndustrialPricePerKilowat).HasColumnType("decimal(18,4)");
        builder.Property(e => e.IndustrialFixedCharge).HasColumnType("decimal(18,4)");
        builder.Property(e => e.IndustrialTVA).HasColumnType("decimal(5,2)");

        builder.Property(e => e.Language).HasMaxLength(10);

        builder.HasOne(e => e.CustomerExportColumnPreference)
            .WithOne()
            .HasForeignKey<CustomerExportColumnPreference>(e => e.AppPreferencesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
