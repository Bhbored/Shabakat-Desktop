using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Infrastructure.Persistence;
using Shabakat.Infrastructure.Persistence.Interceptors;
using Shabakat.Infrastructure.Repository;

public static class DIContainer
{
    public static IServiceCollection RegisterDataBase(this IServiceCollection services)
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Shabakat.db3");
        var connectionString = $"Data Source={databasePath}";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString)
                .AddInterceptors(new AuditInterceptor()));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        return services;
    }

    public static IServiceCollection RegisterDependencies(this IServiceCollection services)
    {
        return services.RegisterDataBase();
    }
}
