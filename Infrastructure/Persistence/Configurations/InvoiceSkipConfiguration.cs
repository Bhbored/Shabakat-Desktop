using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class InvoiceSkipConfiguration : IEntityTypeConfiguration<InvoiceSkip>
{
    public void Configure(EntityTypeBuilder<InvoiceSkip> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Reason).IsRequired().HasMaxLength(500);

        builder.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.CustomerId, e.BillingPeriodStart, e.BillingPeriodEnd })
            .IsUnique()
            .HasDatabaseName("UQ_InvoiceSkips_Customer_Period");
    }
}
