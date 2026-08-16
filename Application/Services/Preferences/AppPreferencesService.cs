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
            throw new DomainException("Error.EnableScheduleNeedsSchedule");
        }

        var existing = await _preferencesRepository.GetAsync();
        var isCreate = existing is null;

        if (existing is null)
        {
            var created = new AppPreferences
            {
                CustomerExportColumnPreference = new CustomerExportColumnPreference()
            }.Apply(request);
            await _preferencesRepository.AddAsync(created);
        }
        else
        {
            existing.EnsureExportColumns();
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
                throw DomainException.Format("Error.CannotBeNegative", name);
        }

        void CheckTva(decimal value, string name)
        {
            if (value < 0 || value > 100)
                throw DomainException.Format("Error.MustBePercent", name);
        }

        CheckNonNegative(request.PricePerKilowat, "Settings.PricePerKilowatt");
        CheckNonNegative(request.PricePerAmp, "Settings.PricePerAmp");
        CheckNonNegative(request.FixedCharge, "Settings.FixedCharge");
        CheckTva(request.TVA, "Settings.Tva");

        CheckNonNegative(request.ResidentialPricePerAmp, "Error.Field.ResidentialPricePerAmp");
        CheckNonNegative(request.ResidentialPricePerKilowat, "Error.Field.ResidentialPricePerKilowatt");
        CheckNonNegative(request.ResidentialFixedCharge, "Error.Field.ResidentialFixedCharge");
        CheckTva(request.ResidentialTVA, "Error.Field.ResidentialTva");

        CheckNonNegative(request.CommercialPricePerAmp, "Error.Field.CommercialPricePerAmp");
        CheckNonNegative(request.CommercialPricePerKilowat, "Error.Field.CommercialPricePerKilowatt");
        CheckNonNegative(request.CommercialFixedCharge, "Error.Field.CommercialFixedCharge");
        CheckTva(request.CommercialTVA, "Error.Field.CommercialTva");

        CheckNonNegative(request.IndustrialPricePerAmp, "Error.Field.IndustrialPricePerAmp");
        CheckNonNegative(request.IndustrialPricePerKilowat, "Error.Field.IndustrialPricePerKilowatt");
        CheckNonNegative(request.IndustrialFixedCharge, "Error.Field.IndustrialFixedCharge");
        CheckTva(request.IndustrialTVA, "Error.Field.IndustrialTva");

        if (string.IsNullOrWhiteSpace(request.Language))
            throw new DomainException("Error.LanguageRequired");
        if (request.Language.Trim().Length > 10)
            throw new DomainException("Error.LanguageTooLong");

        if (request.DueDate < 1 || request.DueDate > 31)
            throw new DomainException("Error.DueDateRange");
    }
}
