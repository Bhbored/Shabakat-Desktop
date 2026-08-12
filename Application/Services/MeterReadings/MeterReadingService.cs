using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.MeterReadings;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.MeterReadings;

public sealed class MeterReadingService : IMeterReadingService
{
    private readonly IMeterReadingRepository _meterReadingRepository;
    private readonly ICustomerRepository _customerRepository;

    public MeterReadingService(
        IMeterReadingRepository meterReadingRepository,
        ICustomerRepository customerRepository)
    {
        _meterReadingRepository = meterReadingRepository;
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<MeterReadingResponse>> GetAllForCustomerAsync(Guid customerId)
    {
        _ = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new DomainException("Customer not found.");

        var readings = (await _meterReadingRepository.GetAllForCustomerAsync(customerId))
            .OrderBy(r => r.ReadingDate)
            .ThenByDescending(r => r.IsInitial)
            .ToList();

        var result = new List<MeterReadingResponse>();
        decimal? previousValue = null;

        foreach (var reading in readings)
        {
            decimal? consumption = previousValue is null
                ? reading.ReadingValue
                : reading.ReadingValue - previousValue.Value;

            result.Add(new MeterReadingResponse(
                Id: reading.Id,
                ReadingValue: reading.ReadingValue,
                Consumption: consumption,
                CreatedAt: reading.CreatedAt));

            previousValue = reading.ReadingValue;
        }

        result.Reverse();
        return result;
    }

    public async Task<IEnumerable<MeterReadingListItemResponse>> GetAllUnpagedAsync()
    {
        var readings = (await _meterReadingRepository.GetAllWithCustomerAsync()).ToList();
        var result = new List<MeterReadingListItemResponse>();

        foreach (var group in readings.GroupBy(r => r.CustomerId))
        {
            var ordered = group
                .OrderBy(r => r.ReadingDate)
                .ThenByDescending(r => r.IsInitial)
                .ToList();
            decimal? previousValue = null;

            foreach (var reading in ordered)
            {
                decimal? consumption = previousValue is null
                    ? reading.ReadingValue
                    : reading.ReadingValue - previousValue.Value;

                result.Add(new MeterReadingListItemResponse(
                    Id: reading.Id,
                    CustomerId: reading.CustomerId,
                    CustomerName: reading.Customer?.Name ?? string.Empty,
                    ReadingValue: reading.ReadingValue,
                    ReadingDate: reading.ReadingDate,
                    Consumption: consumption,
                    CreatedAt: reading.CreatedAt));

                previousValue = reading.ReadingValue;
            }
        }

        return result
            .OrderByDescending(r => r.ReadingDate)
            .ThenByDescending(r => r.CreatedAt);
    }

    public async Task<MeterReadingResponse> GetLatestForCustomerAsync(Guid customerId)
    {
        _ = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new DomainException("Customer not found.");

        var reading = await _meterReadingRepository.GetLatestForCustomerAsync(customerId)
            ?? throw new DomainException("Meter reading not found.");

        var previous = await _meterReadingRepository.GetLatestBeforeAsync(
            customerId, reading.ReadingDate, reading.Id);

        var consumption = previous is null
            ? reading.ReadingValue
            : reading.ReadingValue - previous.ReadingValue;

        return new MeterReadingResponse(
            Id: reading.Id,
            ReadingValue: reading.ReadingValue,
            Consumption: consumption,
            CreatedAt: reading.CreatedAt);
    }

    public async Task<MeterReadingResponse> CreateAsync(
        Guid customerId, CreateMeterReadingRequest request)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId)
            ?? throw new DomainException("Customer not found.");

        if (customer.Plan != PlanType.Kilowatt)
        {
            throw new DomainException(
                "Meter readings can only be recorded for Kilowatt plan customers.");
        }

        var readingDate = request.ReadingDate ?? DateOnly.FromDateTime(DateTime.Now);

        var periodStart = new DateOnly(readingDate.Year, readingDate.Month, 1);
        var periodEnd = periodStart.AddMonths(1);

        var existingForMonth = await _meterReadingRepository.GetForPeriodAsync(
            customerId, periodStart, periodEnd);

        if (existingForMonth is not null)
        {
            throw new DomainException(
                $"A meter reading already exists for {customer.Name} for " +
                $"{periodStart:yyyy-MM} (recorded on {existingForMonth.ReadingDate}, " +
                $"value {existingForMonth.ReadingValue}). " +
                "Delete or correct the existing reading instead of adding a new one.");
        }

        var previous = await _meterReadingRepository.GetLatestBeforeAsync(customerId, readingDate);

        if (previous is not null && request.ReadingValue < previous.ReadingValue)
        {
            throw new DomainException(
                $"New reading ({request.ReadingValue}) cannot be less than the previous reading " +
                $"({previous.ReadingValue}) recorded on {previous.ReadingDate}.");
        }

        var reading = new MeterReading
        {
            CustomerId = customerId,
            ReadingValue = request.ReadingValue,
            ReadingDate = readingDate,
            IsInitial = false
        };

        await _meterReadingRepository.AddAsync(reading);
        await _meterReadingRepository.SaveChangesAsync();

        var consumption = previous is null
            ? reading.ReadingValue
            : reading.ReadingValue - previous.ReadingValue;

        return new MeterReadingResponse(
            Id: reading.Id,
            ReadingValue: reading.ReadingValue,
            Consumption: consumption,
            CreatedAt: reading.CreatedAt);
    }

    public async Task DeleteAsync(Guid customerId, Guid readingId)
    {
        var reading = await _meterReadingRepository.GetByIdAsync(readingId)
            ?? throw new DomainException("Meter reading not found.");

        if (reading.CustomerId != customerId)
            throw new DomainException("Meter reading not found.");

        _meterReadingRepository.Delete(reading);
        await _meterReadingRepository.SaveChangesAsync();
    }
}
