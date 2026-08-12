using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class ExpensesConfiguration : IEntityTypeConfiguration<Expenses>
{
    public void Configure(EntityTypeBuilder<Expenses> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExpenseType).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,4)");
        builder.Property(e => e.Label).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(500);

        builder.ToTable(t => t.HasCheckConstraint("CK_Expenses_Amount", "\"Amount\" > 0"));

        builder.HasIndex(e => e.ExpenseDate);
    }
}
