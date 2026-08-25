using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Backup;
using Shabakat.Application.Options;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Application.Services.Backup;

public sealed class CloudBackupService : ICloudBackupService
{
    private static readonly TimeSpan UploadInterval = TimeSpan.FromDays(7);
    private static readonly TimeSpan ManualUploadCooldown = TimeSpan.FromMinutes(15);
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly SemaphoreSlim CreateGate = new(1, 1);

    private readonly AppDbContext _db;
    private readonly IBackupService _backupService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<CloudBackupOptions> _options;
    private readonly ILogger<CloudBackupService> _logger;

    public CloudBackupService(
        AppDbContext db,
        IBackupService backupService,
        IHttpClientFactory httpClientFactory,
        IOptions<CloudBackupOptions> options,
        ILogger<CloudBackupService> logger)
    {
        _db = db;
        _backupService = backupService;
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    public async Task<CloudBackupStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        return ToStatus(state);
    }

    public async Task SetEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        var state = await GetOrCreateStateAsync(cancellationToken);
        state.Enabled = enabled;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UploadNowAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
            throw new DomainException("Error.CloudBackupNotConfigured");

        var state = await GetOrCreateStateAsync(cancellationToken);
        if (!state.Enabled)
            throw new DomainException("Error.CloudBackupDisabled");

        if (state.LastSuccessfulUploadAt is { } lastSuccess)
        {
            var elapsed = DateTime.UtcNow - lastSuccess;
            if (elapsed < ManualUploadCooldown)
            {
                var remaining = ManualUploadCooldown - elapsed;
                var minutes = Math.Max(1, (int)Math.Ceiling(remaining.TotalMinutes));
                throw DomainException.Format("Error.CloudBackupRateLimited", minutes);
            }
        }

        await UploadCoreAsync(state, cancellationToken);
        if (state.LastError is not null)
            throw new DomainException("Error.CloudBackupFailed");
    }

    public async Task TryScheduledUploadAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
            return;

        var state = await GetOrCreateStateAsync(cancellationToken);
        if (!state.Enabled)
            return;

        if (state.LastSuccessfulUploadAt is { } last
            && DateTime.UtcNow - last < UploadInterval)
            return;

        await UploadCoreAsync(state, cancellationToken);
    }

    private async Task UploadCoreAsync(CloudBackupState state, CancellationToken cancellationToken)
    {
        await Gate.WaitAsync(cancellationToken);
        try
        {
            state.LastAttemptAt = DateTime.UtcNow;
            state.LastError = null;
            await _db.SaveChangesAsync(cancellationToken);

            var jsonBytes = await _backupService.ExportJsonBytesAsync(cancellationToken);
            if (!TryBuildUploadUri(_options.Value.WorkerUrl, out var url))
            {
                SetError(state, "Invalid WorkerUrl");
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogWarning("Cloud backup WorkerUrl is not a valid absolute URI");
                return;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Value.Secret);
            request.Headers.TryAddWithoutValidation("X-Install-Id", state.InstallId.ToString("D"));
            request.Content = new ByteArrayContent(jsonBytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };

            using var client = _httpClientFactory.CreateClient(nameof(CloudBackupService));
            using var response = await client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync(cancellationToken);
                SetError(state, $"HTTP {(int)response.StatusCode} {detail}");
                await _db.SaveChangesAsync(cancellationToken);
                _logger.LogWarning(
                    "Cloud backup upload failed with {StatusCode}",
                    (int)response.StatusCode);
                return;
            }

            state.LastSuccessfulUploadAt = DateTime.UtcNow;
            state.LastObjectKey = response.Headers.TryGetValues("X-Object-Key", out var keys)
                ? keys.FirstOrDefault()
                : null;
            state.LastError = null;
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cloud backup uploaded as {ObjectKey}", state.LastObjectKey);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            SetError(state, ex.Message);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogWarning(ex, "Cloud backup upload failed");
        }
        finally
        {
            Gate.Release();
        }
    }

    private CloudBackupStatusResponse ToStatus(CloudBackupState state) =>
        new(
            Configured: IsConfigured(),
            Enabled: state.Enabled,
            LastSuccessfulUploadAt: state.LastSuccessfulUploadAt,
            LastAttemptAt: state.LastAttemptAt,
            LastObjectKey: state.LastObjectKey,
            LastError: state.LastError);

    private bool IsConfigured()
    {
        var options = _options.Value;
        return TryBuildUploadUri(options.WorkerUrl, out _)
            && !string.IsNullOrWhiteSpace(options.Secret);
    }

    private static bool TryBuildUploadUri(string? workerUrl, out Uri? uri)
    {
        uri = null;
        if (string.IsNullOrWhiteSpace(workerUrl))
            return false;

        var baseUrl = workerUrl.Trim().TrimEnd('/');
        if (!baseUrl.Contains("://", StringComparison.Ordinal))
            baseUrl = "https://" + baseUrl;

        if (!Uri.TryCreate($"{baseUrl}/v1/backups", UriKind.Absolute, out var built)
            || (built.Scheme != Uri.UriSchemeHttps && built.Scheme != Uri.UriSchemeHttp))
            return false;

        uri = built;
        return true;
    }

    private async Task<CloudBackupState> GetOrCreateStateAsync(CancellationToken cancellationToken)
    {
        var state = await _db.CloudBackupStates.FirstOrDefaultAsync(cancellationToken);
        if (state is not null)
            return state;

        await CreateGate.WaitAsync(cancellationToken);
        try
        {
            state = await _db.CloudBackupStates.FirstOrDefaultAsync(cancellationToken);
            if (state is not null)
                return state;

            state = new CloudBackupState
            {
                InstallId = Guid.NewGuid(),
                Enabled = true
            };
            await _db.CloudBackupStates.AddAsync(state, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return state;
        }
        finally
        {
            CreateGate.Release();
        }
    }

    private static void SetError(CloudBackupState state, string message)
    {
        state.LastError = message.Length <= 2000 ? message : message[..2000];
    }
}
