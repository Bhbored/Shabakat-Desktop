using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.InvoiceNumber).IsRequired();
        builder.HasIndex(e => e.InvoiceNumber)
            .IsUnique()
            .HasDatabaseName("UQ_Invoices_InvoiceNumber");

        builder.Property(e => e.InvoiceStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.FixedCharge).HasColumnType("decimal(18,4)");
        builder.Property(e => e.TVA).HasColumnType("decimal(5,2)");
        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,4)");
        builder.Property(e => e.PaidAmount).HasColumnType("decimal(18,4)");
        builder.Property(e => e.BilledConsumption).HasColumnType("decimal(18,4)");

        builder.Property(e => e.AmountDue)
            .HasComputedColumnSql("\"TotalAmount\" - \"PaidAmount\"", stored: true)
            .HasColumnType("decimal(18,4)");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Invoices_PaidAmount",
            "\"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"TotalAmount\""));

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Invoices)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CustomerId);
    }
}
