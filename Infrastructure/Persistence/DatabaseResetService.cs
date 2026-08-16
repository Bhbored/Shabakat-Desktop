using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Services;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Infrastructure.Persistence;

public sealed class DatabaseResetService : IDatabaseResetService
{
    private readonly AppDbContext _db;
    private readonly ICultureService _culture;
    private readonly ILicenseService _license;
    private readonly ILogger<DatabaseResetService> _logger;

    public DatabaseResetService(
        AppDbContext db,
        ICultureService culture,
        ILicenseService license,
        ILogger<DatabaseResetService> logger)
    {
        _db = db;
        _culture = culture;
        _license = license;
        _logger = logger;
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _db.Database.CloseConnectionAsync();
            _db.ChangeTracker.Clear();
            SqliteConnection.ClearAllPools();

            await _db.Database.EnsureDeletedAsync(cancellationToken);
            await _db.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DomainException("Error.DatabaseResetFailed", ex);
        }

        _culture.Apply("en");
        _license.NotifyChanged();
        _logger.LogWarning("Local database file was deleted and recreated empty.");
    }
}
