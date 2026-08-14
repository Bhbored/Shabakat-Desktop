using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Preferences;
using Shabakat.Application.Mappers;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Preferences;

public sealed class AppPreferencesService : IAppPreferencesService
{
    private readonly IAppPreferencesRepository _preferencesRepository;
    private readonly IAmpereScheduleRepository _ampereScheduleRepository;
    private readonly ILogger<AppPreferencesService> _logger;

    public AppPreferencesService(
        IAppPreferencesRepository preferencesRepository,
        IAmpereScheduleRepository ampereScheduleRepository,
        ILogger<AppPreferencesService> logger)
    {
        _preferencesRepository = preferencesRepository;
        _ampereScheduleRepository = ampereScheduleRepository;
        _logger = logger;
    }

    public async Task<GetPreferencesResponse?> GetAsync()
    {
        var preferences = await _preferencesRepository.GetAsync();
        if (preferences is null)
            return null;

        return preferences.ToResponse();
    }

    public async Task UpsertAsync(UpdatePreferencesRequest request)
    {
        ValidatePreferencesRequest(request);

        if (request.AmpereSchedulePricingEnabled &&
            !await _ampereScheduleRepository.HasAnyAsync())
        {
            throw new DomainException(
                "Create at least one ampere schedule before enabling schedule pricing.");
        }

        var existing = await _preferencesRepository.GetAsync();
        var isCreate = existing is null;

        if (existing is null)
        {
            await _preferencesRepository.AddAsync(new AppPreferences().Apply(request));
        }
        else
        {
            existing.Apply(request);
            _preferencesRepository.Update(existing);
        }

        await _preferencesRepository.SaveChangesAsync();
        _logger.LogInformation("{Action} app preferences", isCreate ? "Created" : "Updated");
    }

    private static void ValidatePreferencesRequest(UpdatePreferencesRequest request)
    {
        void CheckNonNegative(decimal value, string name)
        {
            if (value < 0)
                throw new DomainException($"{name} cannot be negative.");
        }

        void CheckTva(decimal value, string name)
        {
            if (value < 0 || value > 100)
                throw new DomainException($"{name} must be between 0 and 100.");
        }

        CheckNonNegative(request.PricePerKilowat, nameof(request.PricePerKilowat));
        CheckNonNegative(request.PricePerAmp, nameof(request.PricePerAmp));
        CheckNonNegative(request.FixedCharge, nameof(request.FixedCharge));
        CheckTva(request.TVA, nameof(request.TVA));

        CheckNonNegative(request.ResidentialPricePerAmp, nameof(request.ResidentialPricePerAmp));
        CheckNonNegative(request.ResidentialPricePerKilowat, nameof(request.ResidentialPricePerKilowat));
        CheckNonNegative(request.ResidentialFixedCharge, nameof(request.ResidentialFixedCharge));
        CheckTva(request.ResidentialTVA, nameof(request.ResidentialTVA));

        CheckNonNegative(request.CommercialPricePerAmp, nameof(request.CommercialPricePerAmp));
        CheckNonNegative(request.CommercialPricePerKilowat, nameof(request.CommercialPricePerKilowat));
        CheckNonNegative(request.CommercialFixedCharge, nameof(request.CommercialFixedCharge));
        CheckTva(request.CommercialTVA, nameof(request.CommercialTVA));

        CheckNonNegative(request.IndustrialPricePerAmp, nameof(request.IndustrialPricePerAmp));
        CheckNonNegative(request.IndustrialPricePerKilowat, nameof(request.IndustrialPricePerKilowat));
        CheckNonNegative(request.IndustrialFixedCharge, nameof(request.IndustrialFixedCharge));
        CheckTva(request.IndustrialTVA, nameof(request.IndustrialTVA));

        if (string.IsNullOrWhiteSpace(request.Language))
            throw new DomainException("Language is required.");
        if (request.Language.Trim().Length > 10)
            throw new DomainException("Language cannot exceed 10 characters.");

        if (request.DueDate < 1 || request.DueDate > 31)
            throw new DomainException("DueDate must be between 1 and 31.");
    }
}
