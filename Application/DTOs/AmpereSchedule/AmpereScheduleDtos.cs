using System.ComponentModel.DataAnnotations;

namespace Shabakat.Application.DTOs.AmpereSchedule;

public record AmpereScheduleResponse(
    Guid Id,
    string Name,
    int HoursPerDay,
    decimal PricePerAmp,
    decimal ResidentialPricePerAmp,
    decimal CommercialPricePerAmp,
    decimal IndustrialPricePerAmp,
    int CustomerCount,
    bool CanBeDeleted,
    DateTime CreatedAt);

public record CreateAmpereScheduleRequest(
    [Required][MaxLength(200)] string Name,
    [Required][Range(1, 24)] int HoursPerDay,
    [Range(0, 9999999)] decimal? PricePerAmp = null,
    [Range(0, 9999999)] decimal? ResidentialPricePerAmp = null,
    [Range(0, 9999999)] decimal? CommercialPricePerAmp = null,
    [Range(0, 9999999)] decimal? IndustrialPricePerAmp = null);

public record UpdateAmpereScheduleRequest(
    [Required][MaxLength(200)] string Name,
    [Required][Range(1, 24)] int HoursPerDay,
    [Range(0, 9999999)] decimal? PricePerAmp = null,
    [Range(0, 9999999)] decimal? ResidentialPricePerAmp = null,
    [Range(0, 9999999)] decimal? CommercialPricePerAmp = null,
    [Range(0, 9999999)] decimal? IndustrialPricePerAmp = null);
