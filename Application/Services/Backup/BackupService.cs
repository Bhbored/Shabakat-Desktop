using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Backup;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Backup;

public sealed class BackupService : IBackupService
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    private readonly IBackupRepository _backupRepository;
    private readonly ICultureService _cultureService;
    private readonly ILogger<BackupService> _logger;

    public BackupService(
        IBackupRepository backupRepository,
        ICultureService cultureService,
        ILogger<BackupService> logger)
    {
        _backupRepository = backupRepository;
        _cultureService = cultureService;
        _logger = logger;
    }

    public async IAsyncEnumerable<double> ExportAsync(
        string destinationPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var file = new BackupFile
        {
            Version = BackupFile.CurrentVersion,
            ExportedAt = DateTime.Now
        };

        await foreach (var progress in _backupRepository.LoadAsync(file, cancellationToken))
            yield return progress * 0.9;

        var json = JsonSerializer.Serialize(file, JsonOptions);
        yield return 0.95;

        await File.WriteAllTextAsync(destinationPath, json, cancellationToken);
        _logger.LogInformation(
            "Exported backup version {Version} at {ExportedAt}",
            file.Version,
            file.ExportedAt);
        yield return 1d;
    }

    public async IAsyncEnumerable<double> RestoreAsync(
        string json,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return 0.02;
        var file = Parse(json);
        if (file.Version != BackupFile.CurrentVersion)
            throw new DomainException("This backup file is not supported.");

        yield return 0.08;

        await foreach (var progress in WithRestoreErrors(_backupRepository.ReplaceAsync(file, cancellationToken)))
            yield return 0.08 + progress * 0.9;

        _cultureService.Apply(file.Preferences?.Language ?? "en");
        _logger.LogInformation("Restored backup version {Version} exported at {ExportedAt}", file.Version, file.ExportedAt);
        yield return 1d;
    }

    private static async IAsyncEnumerable<double> WithRestoreErrors(IAsyncEnumerable<double> source)
    {
        await using var enumerator = source.GetAsyncEnumerator();
        while (true)
        {
            bool moved;
            try
            {
                moved = await enumerator.MoveNextAsync();
            }
            catch (DomainException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DomainException("Could not restore this backup file.", ex);
            }

            if (!moved)
                yield break;

            yield return enumerator.Current;
        }
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
