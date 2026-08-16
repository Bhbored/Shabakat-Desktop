using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.Services.AmpereSchedules;
using Shabakat.Application.Services.Areas;
using Shabakat.Application.Services.AuditLogs;
using Shabakat.Application.Services.Backup;
using Shabakat.Application.Services.Customers;
using Shabakat.Application.Services.Dashboard;
using Shabakat.Application.Services.DistributionBoxes;
using Shabakat.Application.Services.Expense;
using Shabakat.Application.Services.Export;
using Shabakat.Application.Services.Invoices;
using Shabakat.Application.Services.MeterReadings;
using Shabakat.Application.Services.Preferences;
using Shabakat.Application.Services.Pricing;
using Shabakat.Application.Services.Profile;
using Shabakat.Application.Services.License;
using Shabakat.Application.Services.Culture;
using Microsoft.AspNetCore.Identity;
using Shabakat.Application.Services.Theme;
using Shabakat.Application.Services.Toasts;
using Shabakat.Infrastructure.Persistence;
using Shabakat.Infrastructure.Persistence.Interceptors;
using Shabakat.Infrastructure.Persistence.Seed;
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
        services.AddScoped<IAreaRepository, AreaRepository>();
        services.AddScoped<IAmpereScheduleRepository, AmpereScheduleRepository>();
        services.AddScoped<IDistributionBoxRepository, DistributionBoxRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IInvoiceSkipRepository, InvoiceSkipRepository>();
        services.AddScoped<IMeterReadingRepository, MeterReadingRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppPreferencesRepository, AppPreferencesRepository>();
        services.AddScoped<IBackupRepository, BackupRepository>();
        services.AddScoped<ICustomerExportRepository, CustomerExportRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        return services;
    }

    public static IServiceCollection RegisterDependencies(this IServiceCollection services)
    {
        services.RegisterDataBase();

        services.AddLocalization();
        services.AddSingleton<ICultureService, CultureService>();
        services.AddSingleton<IStringLocalizer<Shabakat.Resources.Localization.SharedResource>, SharedResourceLocalizer>();
        services.AddSingleton<IInvoiceTemplateRenderer, InvoiceTemplateRenderer>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddScoped<IToastService, ToastService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IAppPreferencesService, AppPreferencesService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IAppUserService, AppUserService>();
        services.AddSingleton(_ => new PasswordHasher<Shabakat.Domain.Entities.AppUser>());
        services.AddScoped<ILicenseService, LicenseService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAreaService, AreaService>();
        services.AddScoped<IAmpereScheduleService, AmpereScheduleService>();
        services.AddScoped<IDistributionBoxService, DistributionBoxService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IMeterReadingService, MeterReadingService>();
        services.AddSingleton<ICustomerExportWorkbookBuilder, ClosedXmlCustomerExportWorkbookBuilder>();
        services.AddScoped<ICustomerExportService, CustomerExportService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IDatabaseSeedService, LebanonDatabaseSeeder>();
        services.AddScoped<IDatabaseResetService, DatabaseResetService>();

        return services;
    }
}
