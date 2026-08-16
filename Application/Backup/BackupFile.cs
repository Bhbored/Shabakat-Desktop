using Shabakat.Domain.Entities;

namespace Shabakat.Application.Backup;

public sealed class BackupFile
{
    public const int CurrentVersion = 1;

    public int Version { get; set; }
    public DateTime ExportedAt { get; set; }
    public AppPreferences? Preferences { get; set; }
    public List<CustomerExportColumnPreference> ExportColumns { get; set; } = [];
    public List<Area> Areas { get; set; } = [];
    public List<DistributionBox> DistributionBoxes { get; set; } = [];
    public List<AmpereSchedule> AmpereSchedules { get; set; } = [];
    public List<Customer> Customers { get; set; } = [];
    public List<MeterReading> MeterReadings { get; set; } = [];
    public List<Invoice> Invoices { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];
    public List<InvoiceSkip> InvoiceSkips { get; set; } = [];
    public List<Expenses> Expenses { get; set; } = [];
    public List<AuditLog> AuditLogs { get; set; } = [];
    public List<AuditLogDetails> AuditLogDetails { get; set; } = [];
}
