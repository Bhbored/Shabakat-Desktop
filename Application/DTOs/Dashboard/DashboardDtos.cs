namespace Shabakat.Application.DTOs.Dashboard;

public record CustomerOverview(
    int Total,
    int Active,
    int Suspended,
    int Terminated,
    int AmpereCount,
    int KilowattCount);

public record InvoiceOverview(
    int UnpaidCount,
    decimal UnpaidTotal,
    int PartiallyPaidCount,
    decimal PartiallyPaidTotal,
    int PaidCount,
    decimal PaidTotal);

public record ExpensesByType(
    decimal Fuel,
    decimal Maintenance,
    decimal Employees,
    decimal Other);
