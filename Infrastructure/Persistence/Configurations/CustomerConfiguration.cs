using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.Building).HasMaxLength(100);
        builder.Property(e => e.Floor).HasMaxLength(50);
        builder.Property(e => e.CableName).HasMaxLength(100);
        builder.Property(e => e.PlanValue).HasColumnType("decimal(18,4)");
        builder.Property(e => e.Plan).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.CustomerStatus).HasConversion<byte>();
        builder.Property(e => e.CustomerType)
            .HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(e => e.CustomerRelation)
            .HasConversion<string>().HasMaxLength(20).IsRequired(false);

        builder.Property(e => e.PriceOverride).HasColumnType("decimal(18,4)");
        builder.Property(e => e.FixedChargeOverride).HasColumnType("decimal(18,4)");
        builder.Property(e => e.TVAOverride).HasColumnType("decimal(5,2)");

        builder.ToTable(t => t.HasCheckConstraint(
            name: "CK_Customers_PricingOverrides_AllOrNothing",
            sql: """
                (
                    "PriceOverride" IS NULL AND "FixedChargeOverride" IS NULL AND "TVAOverride" IS NULL
                )
                OR
                (
                    "PriceOverride" IS NOT NULL AND "FixedChargeOverride" IS NOT NULL AND "TVAOverride" IS NOT NULL
                )
                """));

        builder.Property(e => e.AreaId).IsRequired(false);

        builder.HasOne(e => e.Area)
            .WithMany(a => a.Customers)
            .HasForeignKey(e => e.AreaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.DistributionBox)
            .WithMany(b => b.Customers)
            .HasForeignKey(e => e.BoxId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.AmpereSchedule)
            .WithMany(s => s.Customers)
            .HasForeignKey(e => e.AmpereScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Name);
        builder.HasIndex(e => e.Phone);
        builder.HasIndex(e => e.AreaId);
        builder.HasIndex(e => e.BoxId);
        builder.HasIndex(e => e.AmpereScheduleId);
    }
}
