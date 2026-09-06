using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class CustomerExportRepository : ICustomerExportRepository
{
    private readonly AppDbContext _db;

    public CustomerExportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ExportAreaRef>> GetAreasAsync(
        IReadOnlyCollection<Guid>? areaIds,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Areas.AsQueryable();

        if (areaIds is { Count: > 0 })
        {
            var ids = areaIds.Distinct().ToList();
            query = query.Where(a => ids.Contains(a.Id));
        }

        return await query
            .OrderBy(a => a.Name)
            .Select(a => new ExportAreaRef(a.Id, a.Name))
            .ToListAsync(cancellationToken);
    }

    public Task<IReadOnlyList<CustomerExportRow>> GetRowsForAreaAsync(
        Guid areaId,
        CancellationToken cancellationToken = default)
        => LoadRowsAsync(
            _db.Customers.Where(c => c.AreaId == areaId),
            cancellationToken);

    public Task<IReadOnlyList<CustomerExportRow>> GetRowsWithoutAreaAsync(
        CancellationToken cancellationToken = default)
        => LoadRowsAsync(
            _db.Customers.Where(c => c.AreaId == null),
            cancellationToken);

    public Task<IReadOnlyList<CustomerExportRow>> GetRowsAsync(
        IReadOnlyCollection<Guid>? areaIds,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Customers.AsQueryable();

        if (areaIds is { Count: > 0 })
        {
            var ids = areaIds.Distinct().ToList();
            query = query.Where(c => c.AreaId.HasValue && ids.Contains(c.AreaId.Value));
        }

        return LoadRowsAsync(query, cancellationToken);
    }

    public async Task<IReadOnlyList<ExportBoxRow>> GetBoxesForAreaAsync(
        Guid areaId,
        CancellationToken cancellationToken = default)
        => await _db.DistributionBoxes
            .Where(b => b.AreaId == areaId)
            .OrderBy(b => b.Name)
            .Select(b => new ExportBoxRow(
                b.Name,
                b.LocationNote,
                b.Notes,
                b.Customers.Count))
            .ToListAsync(cancellationToken);

    private static async Task<IReadOnlyList<CustomerExportRow>> LoadRowsAsync(
        IQueryable<Customer> query,
        CancellationToken cancellationToken)
    {
        var raws = await query
            .Where(c => c.CustomerStatus == CustomerStatus.Active)
            .OrderBy(c => c.Name)
            .Select(c => new RawRow
            {
                Name = c.Name,
                Phone = c.Phone,
                Address = c.Address,
                Building = c.Building,
                Floor = c.Floor,
                CableName = c.CableName,
                AreaName = c.Area != null ? c.Area.Name : null,
                BoxName = c.DistributionBox != null ? c.DistributionBox.Name : null,
                AmpereScheduleName = c.AmpereSchedule != null ? c.AmpereSchedule.Name : null,
                CustomerType = c.CustomerType,
                Plan = c.Plan,
                PlanValue = c.PlanValue,
                SubscriptionDate = c.SubscriptionDate,
                CustomerStatus = c.CustomerStatus,
                CustomerRelation = c.CustomerRelation,
                InitialMeterReading = c.MeterReadings
                    .Where(m => m.IsInitial)
                    .Select(m => (decimal?)m.ReadingValue)
                    .FirstOrDefault(),
                LatestMeterReading = c.MeterReadings
                    .Where(m => !m.IsInitial)
                    .OrderByDescending(m => m.ReadingDate)
                    .ThenByDescending(m => m.CreatedAt)
                    .Select(m => (decimal?)m.ReadingValue)
                    .FirstOrDefault(),
                TotalBilled = c.Invoices.Sum(i => i.TotalAmount),
                TotalPaid = c.Invoices.Sum(i => i.PaidAmount),
                TotalToPay = c.Invoices
                    .Where(i => i.InvoiceStatus != InvoiceStatus.Paid)
                    .Sum(i => i.AmountDue)
            })
            .ToListAsync(cancellationToken);

        return raws.Select(r => new CustomerExportRow(
            Name: r.Name,
            Phone: r.Phone,
            Address: r.Address,
            Building: r.Building,
            Floor: r.Floor,
            CableName: r.CableName,
            AreaName: r.AreaName,
            BoxName: r.BoxName,
            AmpereScheduleName: r.AmpereScheduleName,
            CustomerType: r.CustomerType.ToString(),
            Plan: r.Plan,
            PlanValue: r.PlanValue,
            SubscriptionDate: r.SubscriptionDate,
            CustomerStatus: r.CustomerStatus.ToString(),
            CustomerRelation: r.CustomerRelation?.ToString(),
            InitialMeterReading: r.InitialMeterReading,
            LatestMeterReading: r.LatestMeterReading,
            TotalBilled: r.TotalBilled,
            TotalPaid: r.TotalPaid,
            TotalToPay: r.TotalToPay)).ToList();
    }

    private sealed class RawRow
    {
        public string Name { get; init; } = string.Empty;
        public string? Phone { get; init; }
        public string? Address { get; init; }
        public string? Building { get; init; }
        public string? Floor { get; init; }
        public string? CableName { get; init; }
        public string? AreaName { get; init; }
        public string? BoxName { get; init; }
        public string? AmpereScheduleName { get; init; }
        public CustomerType CustomerType { get; init; }
        public PlanType Plan { get; init; }
        public decimal PlanValue { get; init; }
        public DateOnly SubscriptionDate { get; init; }
        public CustomerStatus CustomerStatus { get; init; }
        public CustomerRelation? CustomerRelation { get; init; }
        public decimal? InitialMeterReading { get; init; }
        public decimal? LatestMeterReading { get; init; }
        public decimal TotalBilled { get; init; }
        public decimal TotalPaid { get; init; }
        public decimal TotalToPay { get; init; }
    }
}
