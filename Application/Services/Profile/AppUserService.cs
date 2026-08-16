using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Profile;
using Shabakat.Application.Mappers;
using Shabakat.Domain.Exceptions;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Application.Services.Profile;

public sealed class AppUserService : IAppUserService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppUserService> _logger;

    public event Action? Changed;

    public AppUserService(AppDbContext db, ILogger<AppUserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ProfileResponse?> GetAsync()
    {
        var user = await _db.AppUsers.AsNoTracking().FirstOrDefaultAsync();
        return user?.ToResponse();
    }

    public async Task<ProfileResponse> UpsertAsync(UpdateProfileRequest request)
    {
        var businessName = string.IsNullOrWhiteSpace(request.BusinessName)
            ? null
            : request.BusinessName.Trim();

        if (businessName is { Length: > 200 })
            throw new DomainException("Error.BusinessNameTooLong");

        var logoUrl = string.IsNullOrWhiteSpace(request.LogoUrl)
            ? null
            : request.LogoUrl.Trim();

        if (logoUrl is { Length: > 500 })
            throw new DomainException("Error.LogoPathTooLong");

        var user = await _db.AppUsers.FirstOrDefaultAsync();
        if (user is null)
            throw new DomainException("Error.ActivateBeforeProfile");

        user.BusinessName = businessName;
        user.LogoUrl = logoUrl;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated company profile {UserId}", user.Id);
        NotifyChanged();
        return user.ToResponse();
    }

    public void NotifyChanged() => Changed?.Invoke();
}
