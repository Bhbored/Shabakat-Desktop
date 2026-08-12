namespace Shabakat.Domain.Enums;

public enum AuditAction
{
    CustomerCreated,
    CustomerUpdated,
    CustomerDeleted,

    InvoiceCreated,
    InvoiceBulkCreated,
    InvoicePaymentRecorded,
    InvoiceFixedKilowattCharge,

    ExpenseCreated,
    ExpenseUpdated,
    ExpenseDeleted
}
