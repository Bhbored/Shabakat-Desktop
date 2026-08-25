using System.Diagnostics;
using System.Text;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Invoices;

public static class InvoicePdfBuilder
{
    public static string CombineHtmlDocuments(IReadOnlyList<string> documents)
    {
        if (documents.Count == 0)
            throw new DomainException("Error.InvoiceExportEmpty");

        var first = documents[0];
        var arabic = first.Contains("lang=\"ar\"", StringComparison.OrdinalIgnoreCase)
            || first.Contains("dir=\"rtl\"", StringComparison.OrdinalIgnoreCase);

        var styles = ExtractAllStyles(first);
        var body = new StringBuilder();
        foreach (var doc in documents)
            body.AppendLine(ExtractInvoiceRoot(doc));

        var lang = arabic ? "ar" : "en";
        var dirAttr = arabic ? "dir=\"rtl\"" : "dir=\"ltr\"";

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine($"<html lang=\"{lang}\" {dirAttr}>");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"utf-8\" />");
        sb.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        sb.AppendLine("  <title>invoices</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine(styles);
        sb.AppendLine("""
                @media print {
                  body {
                    background: #fff !important;
                    padding: 0 !important;
                    display: block !important;
                    min-height: auto !important;
                  }
                  .invoice {
                    width: 100% !important;
                    min-height: auto !important;
                    border: none !important;
                    page-break-after: always;
                    page-break-inside: avoid;
                  }
                  .invoice:last-of-type {
                    page-break-after: auto;
                  }
                }
            """);
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.Append(body);
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");
        return sb.ToString();
    }

    public static async Task WriteHtmlAsPdfAsync(
        string html,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);

        var edge = ResolveEdgePath()
            ?? throw new DomainException("Error.InvoiceExportPdfFailed");

        var tempHtml = Path.Combine(
            Path.GetTempPath(),
            $"shabakat-invoices-{Guid.NewGuid():N}.html");

        await File.WriteAllTextAsync(tempHtml, html, Encoding.UTF8, cancellationToken);
        try
        {
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            var htmlUri = new Uri(tempHtml).AbsoluteUri;
            var psi = new ProcessStartInfo
            {
                FileName = edge,
                Arguments =
                    $"--headless=new --disable-gpu --no-pdf-header-footer --print-to-pdf=\"{destinationPath}\" \"{htmlUri}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            using var process = Process.Start(psi)
                ?? throw new DomainException("Error.InvoiceExportPdfFailed");

            await process.WaitForExitAsync(cancellationToken);

            for (var i = 0; i < 20; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(destinationPath) && new FileInfo(destinationPath).Length > 0)
                    return;
                await Task.Delay(100, cancellationToken);
            }

            throw new DomainException("Error.InvoiceExportPdfFailed");
        }
        finally
        {
            try { File.Delete(tempHtml); } catch { }
        }
    }

    private static string? ResolveEdgePath()
    {
        var candidates = new[]
        {
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Microsoft", "Edge", "Application", "msedge.exe"),
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string ExtractAllStyles(string html)
    {
        var sb = new StringBuilder();
        var idx = 0;
        while (true)
        {
            var tag = html.IndexOf("<style", idx, StringComparison.OrdinalIgnoreCase);
            if (tag < 0)
                break;

            var start = html.IndexOf('>', tag);
            if (start < 0)
                break;
            start++;

            var end = html.IndexOf("</style>", start, StringComparison.OrdinalIgnoreCase);
            if (end < 0)
                break;

            sb.Append(html.AsSpan(start, end - start));
            sb.AppendLine();
            idx = end + 8;
        }

        return sb.ToString();
    }

    private static string ExtractInvoiceRoot(string html)
    {
        const string open = "<div class=\"invoice\">";
        var start = html.IndexOf(open, StringComparison.OrdinalIgnoreCase);
        if (start < 0)
            throw new DomainException("Error.PrintTemplateNotFound");

        var i = start + open.Length;
        var depth = 1;
        while (i < html.Length && depth > 0)
        {
            var nextOpen = html.IndexOf("<div", i, StringComparison.OrdinalIgnoreCase);
            var nextClose = html.IndexOf("</div>", i, StringComparison.OrdinalIgnoreCase);
            if (nextClose < 0)
                break;

            if (nextOpen >= 0 && nextOpen < nextClose)
            {
                depth++;
                i = nextOpen + 4;
            }
            else
            {
                depth--;
                i = nextClose + 6;
                if (depth == 0)
                    return html[start..i];
            }
        }

        throw new DomainException("Error.PrintTemplateNotFound");
    }
}
