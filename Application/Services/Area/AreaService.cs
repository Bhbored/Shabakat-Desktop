using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Area;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Areas;

public sealed class AreaService : IAreaService
{
    private readonly IAreaRepository _areaRepository;
    private readonly ILogger<AreaService> _logger;

    public AreaService(IAreaRepository areaRepository, ILogger<AreaService> logger)
    {
        _areaRepository = areaRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<AreaResponse>> GetAllAsync()
    {
        var areas = await _areaRepository.GetAllWithCustomerCountAsync();
        return areas.Select(MapToResponse);
    }

    public async Task<AreaResponse> CreateAsync(CreateAreaRequest request)
    {
        var area = new Domain.Entities.Area
        {
            Name = request.Name.Trim()
        };

        await _areaRepository.AddAsync(area);
        await _areaRepository.SaveChangesAsync();

        _logger.LogInformation("Created area {AreaId} ({Name})", area.Id, area.Name);
        return MapToResponse(area);
    }

    public async Task<AreaResponse> UpdateAsync(Guid id, UpdateAreaRequest request)
    {
        var area = await _areaRepository.GetByIdAsync(id)
            ?? throw new DomainException("Area not found.");

        area.Name = request.Name.Trim();

        _areaRepository.Update(area);
        await _areaRepository.SaveChangesAsync();

        _logger.LogInformation("Updated area {AreaId} ({Name})", area.Id, area.Name);
        return MapToResponse(area);
    }

    public async Task DeleteAsync(Guid id)
    {
        var area = await _areaRepository.GetByIdAsync(id)
            ?? throw new DomainException("Area not found.");

        if (await _areaRepository.HasCustomersAsync(id))
        {
            throw new DomainException(
                "Cannot delete an area that has customers assigned to it. " +
                "Reassign or remove the customers first.");
        }

        var name = area.Name;
        _areaRepository.Delete(area);
        await _areaRepository.SaveChangesAsync();
        _logger.LogInformation("Deleted area {AreaId} ({Name})", id, name);
    }

    private static AreaResponse MapToResponse(Domain.Entities.Area a) =>
        new(
            Id: a.Id,
            Name: a.Name,
            CustomerCount: a.Customers?.Count ?? 0,
            CreatedAt: a.CreatedAt);
}
