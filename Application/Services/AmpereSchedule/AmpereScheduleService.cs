using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.AmpereSchedule;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.AmpereSchedules;

public sealed class AmpereScheduleService : IAmpereScheduleService
{
    private readonly IAmpereScheduleRepository _ampereScheduleRepository;

    public AmpereScheduleService(IAmpereScheduleRepository ampereScheduleRepository)
    {
        _ampereScheduleRepository = ampereScheduleRepository;
    }

    public async Task<IEnumerable<AmpereScheduleResponse>> GetAllAsync()
    {
        var schedules = await _ampereScheduleRepository.GetAllWithCustomerCountAsync();
        return schedules.Select(MapToResponse);
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
            ?? throw new DomainException("Ampere schedule not found.");

        return MapToResponse(created);
    }

    public async Task<AmpereScheduleResponse> UpdateAsync(Guid id, UpdateAmpereScheduleRequest request)
    {
        var schedule = await _ampereScheduleRepository.GetByIdAsync(id)
            ?? throw new DomainException("Ampere schedule not found.");

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
            ?? throw new DomainException("Ampere schedule not found.");

        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var schedule = await _ampereScheduleRepository.GetByIdAsync(id)
            ?? throw new DomainException("Ampere schedule not found.");

        if (await _ampereScheduleRepository.HasCustomersAsync(id))
        {
            throw new DomainException(
                "Cannot delete an ampere schedule that has customers assigned to it. " +
                "Reassign or remove the customers first.");
        }

        _ampereScheduleRepository.Delete(schedule);
        await _ampereScheduleRepository.SaveChangesAsync();
    }

    private async Task EnsureUniqueHoursPerDayAsync(int hoursPerDay, Guid? excludeId = null)
    {
        if (await _ampereScheduleRepository.HoursPerDayExistsAsync(hoursPerDay, excludeId))
        {
            throw new DomainException(
                $"An ampere schedule with {hoursPerDay} hours per day already exists.");
        }
    }

    private static AmpereScheduleResponse MapToResponse(Domain.Entities.AmpereSchedule schedule)
    {
        var customerCount = schedule.Customers?.Count ?? 0;
        return new(
            Id: schedule.Id,
            Name: schedule.Name,
            HoursPerDay: schedule.HoursPerDay,
            PricePerAmp: schedule.PricePerAmp,
            ResidentialPricePerAmp: schedule.ResidentialPricePerAmp,
            CommercialPricePerAmp: schedule.CommercialPricePerAmp,
            IndustrialPricePerAmp: schedule.IndustrialPricePerAmp,
            CustomerCount: customerCount,
            CanBeDeleted: customerCount == 0,
            CreatedAt: schedule.CreatedAt);
    }
}
