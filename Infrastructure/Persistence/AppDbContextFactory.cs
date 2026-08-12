using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shabakat.Infrastructure.Persistence;

/// <summary>Used only by <c>dotnet ef</c> (MAUI FileSystem is not available in CLI).</summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=shabakat-design.db3")
            .Options;

        return new AppDbContext(options);
    }
}
