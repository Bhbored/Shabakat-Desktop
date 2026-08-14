using Shabakat.Application.DTOs.Preferences;
using Shabakat.Domain.Entities;

namespace Shabakat.Application.Mappers;

public static class PreferencesMapper
{
    public static GetPreferencesResponse ToResponse(this AppPreferences preferences) =>
        new(
            preferences.PricePerKilowat, preferences.PricePerAmp,
            preferences.FixedCharge, preferences.TVA,
            preferences.ResidentialPricePerAmp, preferences.ResidentialPricePerKilowat,
            preferences.ResidentialFixedCharge, preferences.ResidentialTVA,
            preferences.CommercialPricePerAmp, preferences.CommercialPricePerKilowat,
            preferences.CommercialFixedCharge, preferences.CommercialTVA,
            preferences.IndustrialPricePerAmp, preferences.IndustrialPricePerKilowat,
            preferences.IndustrialFixedCharge, preferences.IndustrialTVA,
            preferences.Language, preferences.DueDate,
            preferences.AmpereSchedulePricingEnabled, preferences.AmpereProrateByDaysEnabled);

    public static AppPreferences Apply(this AppPreferences prefs, UpdatePreferencesRequest request)
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
        prefs.AmpereSchedulePricingEnabled = request.AmpereSchedulePricingEnabled;
        prefs.AmpereProrateByDaysEnabled = request.AmpereProrateByDaysEnabled;

        return prefs;
    }

    public static UpdatePreferencesRequest ToUpdate(this GetPreferencesResponse p) =>
        new(
            p.PricePerKilowat,
            p.PricePerAmp,
            p.FixedCharge,
            p.TVA,
            p.ResidentialPricePerAmp,
            p.ResidentialPricePerKilowat,
            p.ResidentialFixedCharge,
            p.ResidentialTVA,
            p.CommercialPricePerAmp,
            p.CommercialPricePerKilowat,
            p.CommercialFixedCharge,
            p.CommercialTVA,
            p.IndustrialPricePerAmp,
            p.IndustrialPricePerKilowat,
            p.IndustrialFixedCharge,
            p.IndustrialTVA,
            p.Language,
            p.DueDate,
            p.AmpereSchedulePricingEnabled,
            p.AmpereProrateByDaysEnabled);

    public static UpdatePreferencesRequest Defaults() =>
        new(
            PricePerKilowat: 0,
            PricePerAmp: 0,
            FixedCharge: 0,
            TVA: 0,
            ResidentialPricePerAmp: 0,
            ResidentialPricePerKilowat: 0,
            ResidentialFixedCharge: 0,
            ResidentialTVA: 0,
            CommercialPricePerAmp: 0,
            CommercialPricePerKilowat: 0,
            CommercialFixedCharge: 0,
            CommercialTVA: 0,
            IndustrialPricePerAmp: 0,
            IndustrialPricePerKilowat: 0,
            IndustrialFixedCharge: 0,
            IndustrialTVA: 0,
            Language: "en",
            DueDate: 3,
            AmpereSchedulePricingEnabled: false,
            AmpereProrateByDaysEnabled: false);

    public static UpdatePreferencesRequest FromExistingOrDefaults(this GetPreferencesResponse? prefs) =>
        prefs is null ? Defaults() : prefs.ToUpdate();
}
