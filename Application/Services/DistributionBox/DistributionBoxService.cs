using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.DistributionBox;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.DistributionBoxes;

public sealed class DistributionBoxService : IDistributionBoxService
{
    private readonly IDistributionBoxRepository _distributionBoxRepository;
    private readonly IAreaRepository _areaRepository;

    public DistributionBoxService(
        IDistributionBoxRepository distributionBoxRepository,
        IAreaRepository areaRepository)
    {
        _distributionBoxRepository = distributionBoxRepository;
        _areaRepository = areaRepository;
    }

    public async Task<PagedResponse<DistributionBoxResponse>> GetAllAsync(
        DistributionBoxFilterRequest filter)
    {
        var (items, totalCount) = await _distributionBoxRepository.GetAllPagedAsync(filter);

        return PagedResponse<DistributionBoxResponse>.Create(
            data: items.Select(MapToResponse),
            totalCount: totalCount,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize);
    }

    public async Task<IEnumerable<DistributionBoxResponse>> GetAllUnpagedAsync()
    {
        var boxes = await _distributionBoxRepository.GetAllWithDetailsAsync();
        return boxes.Select(MapToResponse);
    }

    public async Task<DistributionBoxResponse> CreateAsync(CreateDistributionBoxRequest request)
    {
        await EnsureAreaExistsAsync(request.AreaId);

        var box = new Domain.Entities.DistributionBox
        {
            Name = request.Name.Trim(),
            AreaId = request.AreaId,
            LocationNote = request.LocationNote?.Trim(),
            Notes = request.Notes?.Trim()
        };

        await _distributionBoxRepository.AddAsync(box);
        await _distributionBoxRepository.SaveChangesAsync();

        var created = await _distributionBoxRepository.GetByIdWithDetailsAsync(box.Id)
            ?? throw new DomainException("Distribution box not found.");

        return MapToResponse(created);
    }

    public async Task<DistributionBoxResponse> UpdateAsync(
        Guid id, UpdateDistributionBoxRequest request)
    {
        var box = await _distributionBoxRepository.GetByIdAsync(id)
            ?? throw new DomainException("Distribution box not found.");

        await EnsureAreaExistsAsync(request.AreaId);

        box.Name = request.Name.Trim();
        box.AreaId = request.AreaId;
        box.LocationNote = request.LocationNote?.Trim();
        box.Notes = request.Notes?.Trim();

        _distributionBoxRepository.Update(box);
        await _distributionBoxRepository.SaveChangesAsync();

        var updated = await _distributionBoxRepository.GetByIdWithDetailsAsync(id)
            ?? throw new DomainException("Distribution box not found.");

        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var box = await _distributionBoxRepository.GetByIdAsync(id)
            ?? throw new DomainException("Distribution box not found.");

        if (await _distributionBoxRepository.HasCustomersAsync(id))
        {
            throw new DomainException(
                "Cannot delete a distribution box that has customers assigned to it. " +
                "Reassign or remove the customers first.");
        }

        _distributionBoxRepository.Delete(box);
        await _distributionBoxRepository.SaveChangesAsync();
    }

    private async Task EnsureAreaExistsAsync(Guid areaId)
    {
        _ = await _areaRepository.GetByIdAsync(areaId)
            ?? throw new DomainException("Area not found.");
    }

    private static DistributionBoxResponse MapToResponse(Domain.Entities.DistributionBox box) =>
        new(
            Id: box.Id,
            Name: box.Name,
            AreaId: box.AreaId,
            AreaName: box.Area?.Name ?? string.Empty,
            LocationNote: box.LocationNote,
            Notes: box.Notes,
            CustomerCount: box.Customers?.Count ?? 0,
            CreatedAt: box.CreatedAt);
}
