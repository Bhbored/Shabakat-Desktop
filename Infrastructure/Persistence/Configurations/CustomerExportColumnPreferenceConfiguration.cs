using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shabakat.Domain.Entities;

namespace Shabakat.Infrastructure.Persistence.Configurations;

public class CustomerExportColumnPreferenceConfiguration : IEntityTypeConfiguration<CustomerExportColumnPreference>
{
    public void Configure(EntityTypeBuilder<CustomerExportColumnPreference> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => e.AppPreferencesId).IsUnique();
        builder.ToTable("CustomerExportColumnPreferences");
    }
}
