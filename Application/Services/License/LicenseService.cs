using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Application.Services.License;

public sealed class LicenseService : ILicenseService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<AppUser> _hasher;
    private readonly ILogger<LicenseService> _logger;

    public event Action? Changed;

    private LicenseRestoreNotice _restoreNotice;

    public LicenseService(
        AppDbContext db,
        PasswordHasher<AppUser> hasher,
        ILogger<LicenseService> logger)
    {
        _db = db;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<LicenseStatus> GetStatusAsync()
    {
        var user = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync();
        return Read(user).Status;
    }

    public async Task<LicenseRemaining?> GetRemainingAsync()
    {
        var user = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync();
        if (Read(user).Status != LicenseStatus.Active || user is null)
            return null;

        var until = BeirutTime.ToLocal(user.LicensedUntil);
        var now = BeirutTime.ToLocal(DateTimeOffset.UtcNow);
        var daysLeft = Math.Max(0, (int)Math.Ceiling((until - now).TotalDays));
        return new LicenseRemaining(daysLeft, DateOnly.FromDateTime(until.DateTime));
    }

    public async Task SetupAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime)
    {
        EnsurePin(pin);
        var licensedUntil = ToFutureUtc(expiryDate, expiryTime);

        var user = await _db.AppUsers.FirstOrDefaultAsync();
        if (user is not null && !string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new DomainException("Error.AlreadyActivated");

        if (user is null)
        {
            user = new AppUser();
            await _db.AppUsers.AddAsync(user);
        }

        user.PasswordHash = _hasher.HashPassword(user, pin.Trim());
        user.LicensedUntil = licensedUntil;
        user.LicenseStamp = LicenseHmac.Compute(user.PasswordHash, user.LicensedUntil);
        await _db.SaveChangesAsync();
        _logger.LogInformation("License set up until {LicensedUntil}", licensedUntil);
        Changed?.Invoke();
    }

    public async Task RenewAsync(string pin, DateOnly expiryDate, TimeOnly expiryTime)
    {
        EnsurePin(pin);
        var licensedUntil = ToFutureUtc(expiryDate, expiryTime);

        var user = await _db.AppUsers.FirstOrDefaultAsync();
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            throw new DomainException("Error.NotActivated");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, pin.Trim());
        if (result == PasswordVerificationResult.Failed)
            throw new DomainException("Error.InvalidActivationKey");

        user.LicensedUntil = licensedUntil;
        user.LicenseStamp = LicenseHmac.Compute(user.PasswordHash, user.LicensedUntil);
        await _db.SaveChangesAsync();
        _logger.LogInformation("License renewed until {LicensedUntil}", licensedUntil);
        Changed?.Invoke();
    }

    public async Task NotifyRestoredAsync()
    {
        var user = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync();
        var (status, notice) = Read(user);
        if (status != LicenseStatus.Active)
            _restoreNotice = notice;
        Changed?.Invoke();
    }

    public LicenseRestoreNotice TakeRestoreNotice()
    {
        var notice = _restoreNotice;
        _restoreNotice = LicenseRestoreNotice.None;
        return notice;
    }

    public void NotifyChanged() => Changed?.Invoke();

    private (LicenseStatus Status, LicenseRestoreNotice Notice) Read(AppUser? user)
    {
        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
            return (LicenseStatus.SetupRequired, LicenseRestoreNotice.Missing);

        if (!LicenseHmac.Matches(user.LicenseStamp, user.PasswordHash, user.LicensedUntil))
        {
            _logger.LogWarning("License stamp is missing or does not match the stored expiry.");
            return (LicenseStatus.Expired, LicenseRestoreNotice.Tampered);
        }

        if (DateTimeOffset.UtcNow < user.LicensedUntil)
            return (LicenseStatus.Active, LicenseRestoreNotice.None);

        return (LicenseStatus.Expired, LicenseRestoreNotice.Expired);
    }

    private static void EnsurePin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Trim().Length < 4)
            throw new DomainException("Error.ActivationKeyTooShort");
    }

    private static DateTimeOffset ToFutureUtc(DateOnly expiryDate, TimeOnly expiryTime)
    {
        var until = BeirutTime.ToUtc(expiryDate, expiryTime);
        if (until <= DateTimeOffset.UtcNow)
            throw new DomainException("Error.ExpiryMustBeFuture");
        return until;
    }
}
