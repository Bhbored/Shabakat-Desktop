using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class UnpaidInvoiceExportRepository : IUnpaidInvoiceExportRepository
{
    private readonly AppDbContext _db;

    public UnpaidInvoiceExportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<UnpaidInvoiceExportRow>> GetOutstandingRowsAsync(
        int paymentDueDay,
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.AmountDue > 0)
            .Select(invoice => new RawRow
            {
                InvoiceNumber = invoice.InvoiceNumber,
                ConsumptionStart = invoice.IssueDate,
                ConsumptionEnd = invoice.DueDate,
                CreatedAt = invoice.CreatedAt,
                InvoiceStatus = invoice.InvoiceStatus,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                AmountDue = invoice.AmountDue,
                BilledConsumption = invoice.BilledConsumption,
                FixedCharge = invoice.FixedCharge,
                TVA = invoice.TVA,
                CustomerName = invoice.Customer.Name,
                CustomerPhone = invoice.Customer.Phone,
                Address = invoice.Customer.Address,
                Building = invoice.Customer.Building,
                Floor = invoice.Customer.Floor,
                CableName = invoice.Customer.CableName,
                AreaName = invoice.Customer.Area != null ? invoice.Customer.Area.Name : null,
                BoxName = invoice.Customer.DistributionBox != null ? invoice.Customer.DistributionBox.Name : null,
                AmpereScheduleName = invoice.Customer.AmpereSchedule != null ? invoice.Customer.AmpereSchedule.Name : null,
                CustomerType = invoice.Customer.CustomerType,
                Plan = invoice.Customer.Plan,
                PlanValue = invoice.Customer.PlanValue,
                CustomerStatus = invoice.Customer.CustomerStatus,
                CustomerRelation = invoice.Customer.CustomerRelation
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new UnpaidInvoiceExportRow(
                row.InvoiceNumber,
                row.ConsumptionStart,
                row.ConsumptionEnd,
                BillingPeriodHelper.ResolvePaymentDueDate(DateOnly.FromDateTime(row.CreatedAt), paymentDueDay),
                row.InvoiceStatus.ToString(),
                row.TotalAmount,
                row.PaidAmount,
                row.AmountDue,
                row.BilledConsumption,
                row.FixedCharge,
                row.TVA,
                row.CustomerName,
                row.CustomerPhone,
                row.Address,
                row.Building,
                row.Floor,
                row.CableName,
                row.AreaName,
                row.BoxName,
                row.AmpereScheduleName,
                row.CustomerType,
                row.Plan,
                row.PlanValue,
                row.CustomerStatus,
                row.CustomerRelation))
            .OrderBy(row => row.PaymentDueDate)
            .ThenBy(row => row.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(row => row.InvoiceNumber)
            .ToList();
    }

    private sealed class RawRow
    {
        public int InvoiceNumber { get; init; }
        public DateOnly ConsumptionStart { get; init; }
        public DateOnly ConsumptionEnd { get; init; }
        public DateTime CreatedAt { get; init; }
        public InvoiceStatus InvoiceStatus { get; init; }
        public decimal TotalAmount { get; init; }
        public decimal PaidAmount { get; init; }
        public decimal AmountDue { get; init; }
        public decimal? BilledConsumption { get; init; }
        public decimal FixedCharge { get; init; }
        public decimal TVA { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string? CustomerPhone { get; init; }
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
        public CustomerStatus CustomerStatus { get; init; }
        public CustomerRelation? CustomerRelation { get; init; }
    }
}
