using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Preferences;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Preferences;

public sealed class AppPreferencesService : IAppPreferencesService
{
    private readonly IAppPreferencesRepository _preferencesRepository;
    private readonly IAmpereScheduleRepository _ampereScheduleRepository;

    public AppPreferencesService(
        IAppPreferencesRepository preferencesRepository,
        IAmpereScheduleRepository ampereScheduleRepository)
    {
        _preferencesRepository = preferencesRepository;
        _ampereScheduleRepository = ampereScheduleRepository;
    }

    public async Task<GetPreferencesResponse?> GetAsync()
    {
        var preferences = await _preferencesRepository.GetAsync();
        if (preferences is null)
            return null;

        return MapToResponse(preferences);
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

        if (existing is null)
        {
            await _preferencesRepository.AddAsync(ApplyRequest(new AppPreferences(), request));
        }
        else
        {
            ApplyRequest(existing, request);
            _preferencesRepository.Update(existing);
        }

        await _preferencesRepository.SaveChangesAsync();
    }

    private static AppPreferences ApplyRequest(AppPreferences prefs, UpdatePreferencesRequest request)
    {
        prefs.PricePerKilowat = request.PricePerKilowat;
        prefs.PricePerAmp = request.PricePerAmp;
        prefs.FixedCharge = request.FixedCharge;
        prefs.TVA = request.TVA;

        prefs.ResidentialPricePerAmp = request.ResidentialPricePerAmp;
        prefs.ResidentialPricePerKilowat = request.ResidentialPricePerKilowat;
        prefs.ResidentialFixedCharge = request.ResidentialFixedCharge;
        prefs.ResidentialTVA = request.ResidentialTVA;

        prefs.CommercialPricePerAmp = request.CommercialPricePerAmp;
        prefs.CommercialPricePerKilowat = request.CommercialPricePerKilowat;
        prefs.CommercialFixedCharge = request.CommercialFixedCharge;
        prefs.CommercialTVA = request.CommercialTVA;

        prefs.IndustrialPricePerAmp = request.IndustrialPricePerAmp;
        prefs.IndustrialPricePerKilowat = request.IndustrialPricePerKilowat;
        prefs.IndustrialFixedCharge = request.IndustrialFixedCharge;
        prefs.IndustrialTVA = request.IndustrialTVA;

        prefs.Language = request.Language.Trim();
        prefs.DueDate = request.DueDate;
        prefs.TriggerDate = request.TriggerDate;
        prefs.TriggerMessage = string.IsNullOrWhiteSpace(request.TriggerMessage)
            ? null
            : request.TriggerMessage.Trim();
        prefs.AmpereSchedulePricingEnabled = request.AmpereSchedulePricingEnabled;
        prefs.AmpereProrateByDaysEnabled = request.AmpereProrateByDaysEnabled;

        return prefs;
    }

    private static GetPreferencesResponse MapToResponse(AppPreferences preferences) =>
        new(
            preferences.PricePerKilowat, preferences.PricePerAmp,
            preferences.FixedCharge, preferences.TVA,
            preferences.ResidentialPricePerAmp, preferences.ResidentialPricePerKilowat,
            preferences.ResidentialFixedCharge, preferences.ResidentialTVA,
            preferences.CommercialPricePerAmp, preferences.CommercialPricePerKilowat,
            preferences.CommercialFixedCharge, preferences.CommercialTVA,
            preferences.IndustrialPricePerAmp, preferences.IndustrialPricePerKilowat,
            preferences.IndustrialFixedCharge, preferences.IndustrialTVA,
            preferences.Language, preferences.DueDate, preferences.TriggerDate,
            preferences.TriggerMessage,
            preferences.AmpereSchedulePricingEnabled, preferences.AmpereProrateByDaysEnabled);

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

        if (request.TriggerDate < 1 || request.TriggerDate > 31)
            throw new DomainException("TriggerDate must be between 1 and 31.");

        if (request.TriggerMessage is not null && request.TriggerMessage.Trim().Length > 1000)
            throw new DomainException("TriggerMessage cannot exceed 1000 characters.");
    }
}
