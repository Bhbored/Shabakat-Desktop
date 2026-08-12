using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.PaymentMethod)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Payments_Amount",
            "\"Amount\" > 0"));

        builder.HasOne(e => e.Customer)
            .WithMany(c => c.Payments)
            .HasForeignKey(e => e.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(e => e.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.CustomerId);
        builder.HasIndex(e => e.InvoiceId);
    }
}
