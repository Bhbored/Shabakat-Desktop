using Shabakat.Application.DTOs.Invoices;
using Shabakat.Application.DTOs.Payment;
using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Contracts.Services;

public interface IInvoiceService
{
    Task<PagedResponse<InvoiceSummaryResponse>> GetAllAsync(InvoiceFilterRequest filter);
    Task<IEnumerable<InvoiceResponse>> GetAllUnpagedAsync();
    Task<InvoiceResponse> GetByIdAsync(Guid id);
    Task CreateAsync(CreateInvoiceRequest request);
    Task<BulkCreateInvoiceResponse> BulkCreateAsync(PlanType? planType = null);
    Task PayAsync(Guid invoiceId, AddPaymentRequest request);
    Task<InvoiceResponse> UpdateAsync(Guid id, UpdateInvoiceRequest request);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<PaymentResponse>> GetPaymentsAsync(Guid invoiceId);
    Task<IEnumerable<PaymentResponse>> GetAllPaymentsUnpagedAsync();
    Task<IEnumerable<InvoiceSkippedResponse>> GetSkippedAsync();
    Task<FixedKilowattCalculateResponse> CalculateFixedKilowattAsync(FixedKilowattCalculateRequest request);
    Task<string> RenderPrintHtmlAsync(Guid invoiceId);
    Task SaveInvoicePdfAsync(Guid invoiceId, string destinationPath);
    IAsyncEnumerable<double> ExportBillingRunPdfAsync(
        int year,
        int month,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
