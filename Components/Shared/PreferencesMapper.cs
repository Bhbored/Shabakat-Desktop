using Shabakat.Application.DTOs.Preferences;

namespace Shabakat.Components.Shared;

public static class PreferencesMapper
{
    public static UpdatePreferencesRequest ToUpdate(GetPreferencesResponse p) =>
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
            p.TriggerDate,
            p.TriggerMessage,
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
            DueDate: 31,
            TriggerDate: 1,
            TriggerMessage: null,
            AmpereSchedulePricingEnabled: false,
            AmpereProrateByDaysEnabled: false);

    public static UpdatePreferencesRequest FromExistingOrDefaults(GetPreferencesResponse? prefs) =>
        prefs is null ? Defaults() : ToUpdate(prefs);
}
