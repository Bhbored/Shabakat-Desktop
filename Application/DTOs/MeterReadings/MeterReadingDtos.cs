using System.ComponentModel.DataAnnotations;

namespace Shabakat.Application.DTOs.MeterReadings;

public record MeterReadingResponse(
    Guid Id,
    decimal ReadingValue,
    decimal? Consumption,
    DateTime CreatedAt);

public record CreateMeterReadingRequest(
    [Required][Range(0, double.MaxValue)] decimal ReadingValue,
    DateOnly? ReadingDate = null);

public record MeterReadingListItemResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    decimal ReadingValue,
    DateOnly ReadingDate,
    decimal? Consumption,
    DateTime CreatedAt);
