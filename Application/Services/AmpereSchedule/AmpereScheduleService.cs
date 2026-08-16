using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.AmpereSchedule;
using Shabakat.Application.Mappers;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.AmpereSchedules;

public sealed class AmpereScheduleService : IAmpereScheduleService
{
    private readonly IAmpereScheduleRepository _ampereScheduleRepository;
    private readonly ILogger<AmpereScheduleService> _logger;

    public AmpereScheduleService(
        IAmpereScheduleRepository ampereScheduleRepository,
        ILogger<AmpereScheduleService> logger)
    {
        _ampereScheduleRepository = ampereScheduleRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AmpereScheduleResponse>> GetAllAsync()
    {
        var schedules = await _ampereScheduleRepository.GetAllWithCustomerCountAsync();
        return schedules.Select(s => s.ToResponse());
    }

    public async Task<AmpereScheduleResponse> CreateAsync(CreateAmpereScheduleRequest request)
    {
        await EnsureUniqueHoursPerDayAsync(request.HoursPerDay);

        var schedule = new Domain.Entities.AmpereSchedule
        {
            Name = request.Name.Trim(),
            HoursPerDay = request.HoursPerDay,
            PricePerAmp = request.PricePerAmp ?? 0m,
            ResidentialPricePerAmp = request.ResidentialPricePerAmp ?? 0m,
            CommercialPricePerAmp = request.CommercialPricePerAmp ?? 0m,
            IndustrialPricePerAmp = request.IndustrialPricePerAmp ?? 0m
        };

        await _ampereScheduleRepository.AddAsync(schedule);
        await _ampereScheduleRepository.SaveChangesAsync();

        var created = await _ampereScheduleRepository.GetByIdWithDetailsAsync(schedule.Id)
            ?? throw new DomainException("Error.ScheduleNotFound");

        _logger.LogInformation(
            "Created ampere schedule {ScheduleId} ({Name}, {HoursPerDay}h/day)",
            created.Id,
            created.Name,
            created.HoursPerDay);

        return created.ToResponse();
    }

    public async Task<AmpereScheduleResponse> UpdateAsync(Guid id, UpdateAmpereScheduleRequest request)
    {
        var schedule = await _ampereScheduleRepository.GetByIdAsync(id)
            ?? throw new DomainException("Error.ScheduleNotFound");

        await EnsureUniqueHoursPerDayAsync(request.HoursPerDay, id);

        schedule.Name = request.Name.Trim();
        schedule.HoursPerDay = request.HoursPerDay;
        schedule.PricePerAmp = request.PricePerAmp ?? 0m;
        schedule.ResidentialPricePerAmp = request.ResidentialPricePerAmp ?? 0m;
        schedule.CommercialPricePerAmp = request.CommercialPricePerAmp ?? 0m;
        schedule.IndustrialPricePerAmp = request.IndustrialPricePerAmp ?? 0m;

        _ampereScheduleRepository.Update(schedule);
        await _ampereScheduleRepository.SaveChangesAsync();

        var updated = await _ampereScheduleRepository.GetByIdWithDetailsAsync(id)
            ?? throw new DomainException("Error.ScheduleNotFound");

        _logger.LogInformation("Updated ampere schedule {ScheduleId} ({Name})", updated.Id, updated.Name);
        return updated.ToResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        var schedule = await _ampereScheduleRepository.GetByIdAsync(id)
            ?? throw new DomainException("Error.ScheduleNotFound");

        if (await _ampereScheduleRepository.HasCustomersAsync(id))
        {
            throw new DomainException("Error.CannotDeleteScheduleWithCustomers");
        }

        var name = schedule.Name;
        _ampereScheduleRepository.Delete(schedule);
        await _ampereScheduleRepository.SaveChangesAsync();
        _logger.LogInformation("Deleted ampere schedule {ScheduleId} ({Name})", id, name);
    }

    private async Task EnsureUniqueHoursPerDayAsync(int hoursPerDay, Guid? excludeId = null)
    {
        if (await _ampereScheduleRepository.HoursPerDayExistsAsync(hoursPerDay, excludeId))
        {
            throw DomainException.Format("Error.DuplicateScheduleHours", hoursPerDay);
        }
    }
}
