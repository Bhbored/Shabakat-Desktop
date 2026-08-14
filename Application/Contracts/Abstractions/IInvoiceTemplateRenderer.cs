using Shabakat.Application.DTOs.Invoices;

namespace Shabakat.Application.Contracts.Abstractions;

public interface IInvoiceTemplateRenderer
{
    string Render(InvoicePrintModel model, string language = "en");
}
