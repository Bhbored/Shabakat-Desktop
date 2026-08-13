using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReadingValue).HasColumnType("decimal(18,4)");
        builder.Property(e => e.ReadingDate).IsRequired();

        builder.Property(e => e.IsInitial)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.MeterReadings)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property<int>("PeriodYearMonth")
            .HasComputedColumnSql(
                "(CAST(strftime('%Y', \"ReadingDate\") AS INTEGER) * 100 + CAST(strftime('%m', \"ReadingDate\") AS INTEGER))",
                stored: true);

        builder.HasIndex("CustomerId", "PeriodYearMonth")
            .IsUnique()
            .HasDatabaseName("UQ_MeterReadings_CustomerId_PeriodYearMonth")
            .HasFilter("\"IsInitial\" = 0");

        builder.HasIndex(e => e.CustomerId)
            .IsUnique()
            .HasDatabaseName("UQ_MeterReadings_CustomerId_IsInitial")
            .HasFilter("\"IsInitial\" = 1");
    }
}
