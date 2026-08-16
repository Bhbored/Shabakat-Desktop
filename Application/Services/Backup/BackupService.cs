using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.Backup;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Backup;

public sealed class BackupService : IBackupService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly IBackupRepository _backupRepository;
    private readonly ICultureService _cultureService;
    private readonly IAppUserService _appUserService;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        IBackupRepository backupRepository,
        ICultureService cultureService,
        IAppUserService appUserService,
        ILogger<BackupService> logger)
    {
        _backupRepository = backupRepository;
        _cultureService = cultureService;
        _appUserService = appUserService;
        _logger = logger;
    }

    public async Task<string> ExportAsync()
    {
        var file = await _backupRepository.LoadAsync();
        file.Version = BackupFile.CurrentVersion;
        file.ExportedAt = DateTime.Now;
        _logger.LogInformation(
            "Exported backup version {Version} at {ExportedAt}",
            file.Version,
            file.ExportedAt);
        return JsonSerializer.Serialize(file, JsonOptions);
    }

    public async Task RestoreAsync(string json)
    {
        var file = Parse(json);
        if (file.Version != BackupFile.CurrentVersion)
            throw new DomainException("This backup file is not supported.");

        try
        {
            await _backupRepository.ReplaceAsync(file);
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new DomainException("Could not restore this backup file.", ex);
        }

        _cultureService.Apply(file.Preferences?.Language ?? "en");
        _appUserService.NotifyChanged();
        _logger.LogInformation("Restored backup version {Version} exported at {ExportedAt}", file.Version, file.ExportedAt);
    }

    private static BackupFile Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new DomainException("This backup file is not valid.");

        try
        {
            var file = JsonSerializer.Deserialize<BackupFile>(json, JsonOptions);
            if (file is null)
                throw new DomainException("This backup file is not valid.");

            file.ExportColumns ??= [];
            file.Areas ??= [];
            file.DistributionBoxes ??= [];
            file.AmpereSchedules ??= [];
            file.Customers ??= [];
            file.MeterReadings ??= [];
            file.Invoices ??= [];
            file.Payments ??= [];
            file.InvoiceSkips ??= [];
            file.Expenses ??= [];
            file.AuditLogs ??= [];
            file.AuditLogDetails ??= [];
            return file;
        }
        catch (DomainException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new DomainException("This backup file is not valid.", ex);
        }
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var resolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { IgnoreNavigationsAndComputed }
        };

        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            TypeInfoResolver = resolver,
            Converters = { new JsonStringEnumConverter() }
        };
    }


    private static void IgnoreNavigationsAndComputed(JsonTypeInfo info)
    {
        if (info.Kind != JsonTypeInfoKind.Object)
            return;
        if (info.Type.Namespace != "Shabakat.Domain.Entities")
            return;

        foreach (var prop in info.Properties)
        {
            if (IsComputed(prop.Name) || !IsPersistedScalar(prop.PropertyType))
            {
                prop.ShouldSerialize = static (_, _) => false;
                prop.Set = null;
            }
        }
    }

    private static bool IsComputed(string jsonName) =>
        jsonName.Equals("amountDue", StringComparison.OrdinalIgnoreCase)
        || jsonName.Equals("hasPricingOverride", StringComparison.OrdinalIgnoreCase);

    private static bool IsPersistedScalar(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(Guid)
            || type == typeof(DateTime)
            || type == typeof(DateOnly)
            || type == typeof(DateTimeOffset);
    }
}
