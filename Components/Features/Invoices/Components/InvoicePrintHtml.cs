using System.Net;
using Shabakat.Application.DTOs.Invoices;
using Shabakat.Components.Shared;

namespace Shabakat.Components.Features.Invoices.Components;

internal static class InvoicePrintHtml
{
    public static string Build(InvoiceResponse invoice)
    {
        var payments = invoice.Payments.ToList();
        var paymentRows = payments.Count == 0
            ? "<p>No payments recorded yet.</p>"
            : string.Join("", payments.Select(p =>
                $"<tr><td>{Encode(FormatHelper.Currency(p.Amount))}</td><td>{Encode(p.PaymentMethod)}</td><td>{Encode(FormatHelper.Date(p.PaymentDate))}</td><td>{Encode(p.Notes ?? "")}</td></tr>"));

        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="utf-8" />
              <title>Invoice #{{invoice.InvoiceNumber}}</title>
              <style>
                body { font-family: Inter, Segoe UI, sans-serif; color: #111; padding: 32px; }
                h1 { font-size: 22px; margin: 0 0 4px; }
                p { margin: 0 0 8px; color: #555; }
                table { width: 100%; border-collapse: collapse; margin-top: 16px; }
                th, td { text-align: left; padding: 8px 10px; border-bottom: 1px solid #ddd; font-size: 13px; }
                th { color: #666; font-size: 11px; letter-spacing: 0.12em; text-transform: uppercase; }
                .totals { margin-top: 24px; width: 280px; margin-left: auto; }
                .totals td { border: 0; padding: 4px 0; }
                .totals td:last-child { text-align: right; font-variant-numeric: tabular-nums; }
              </style>
            </head>
            <body>
              <h1>Invoice #{{invoice.InvoiceNumber}}</h1>
              <p>{{Encode(invoice.CustomerName)}} · Issued {{Encode(FormatHelper.Date(invoice.ConsumptionStart))}}</p>
              <table class="totals">
                <tr><td>Total Amount</td><td>{{Encode(FormatHelper.Currency(invoice.TotalAmount))}}</td></tr>
                <tr><td>Paid Amount</td><td>{{Encode(FormatHelper.Currency(invoice.PaidAmount))}}</td></tr>
                <tr><td>Amount Due</td><td>{{Encode(FormatHelper.Currency(invoice.AmountDue))}}</td></tr>
                <tr><td>Issue Date</td><td>{{Encode(FormatHelper.Date(invoice.ConsumptionStart))}}</td></tr>
                <tr><td>Due Date</td><td>{{Encode(FormatHelper.Date(invoice.ConsumptionEnd))}}</td></tr>
                <tr><td>Status</td><td>{{Encode(invoice.InvoiceStatus)}}</td></tr>
              </table>
              <h2 style="font-size:14px;margin-top:32px;">Payments</h2>
              <table>
                <thead><tr><th>Amount</th><th>Method</th><th>Date</th><th>Notes</th></tr></thead>
                <tbody>{{paymentRows}}</tbody>
              </table>
            </body>
            </html>
            """;
    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
