using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Profile;
using Shabakat.Application.Mappers;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Application.Services.Profile;

public sealed class AppUserService : IAppUserService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AppUserService> _logger;

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
        var fullName = request.FullName?.Trim() ?? string.Empty;
        var username = request.Username?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username is required.");

        if (fullName.Length > 200)
            throw new DomainException("Full name cannot exceed 200 characters.");

        if (username.Length > 100)
            throw new DomainException("Username cannot exceed 100 characters.");

        var businessName = string.IsNullOrWhiteSpace(request.BusinessName)
            ? null
            : request.BusinessName.Trim();

        if (businessName is { Length: > 200 })
            throw new DomainException("Business name cannot exceed 200 characters.");

        var logoUrl = string.IsNullOrWhiteSpace(request.LogoUrl)
            ? null
            : request.LogoUrl.Trim();

        if (logoUrl is { Length: > 500 })
            throw new DomainException("Logo path cannot exceed 500 characters.");

        var user = await _db.AppUsers.FirstOrDefaultAsync();
        var isCreate = user is null;
        if (user is null)
        {
            user = new AppUser
            {
                FullName = fullName,
                Username = username,
                BusinessName = businessName,
                LogoUrl = logoUrl,
                PasswordHash = string.Empty
            };
            await _db.AppUsers.AddAsync(user);
        }
        else
        {
            user.FullName = fullName;
            user.Username = username;
            user.BusinessName = businessName;
            user.LogoUrl = logoUrl;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation(
            "{Action} profile {UserId} ({Username})",
            isCreate ? "Created" : "Updated",
            user.Id,
            user.Username);
        return user.ToResponse();
    }
}
