using ClosedXML.Excel;
using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Services.Export;

public sealed class ClosedXmlUnpaidInvoiceExportWorkbookBuilder : IUnpaidInvoiceExportWorkbookBuilder
{
    private const string DateFormat = "yyyy-mm-dd";
    private const string MoneyFormat = "#,##0.00";
    private const string HeaderFill = "#F2F2F2";

    public byte[] Build(
        IReadOnlyList<UnpaidInvoiceExportRow> rows,
        IReadOnlyList<UnpaidInvoiceExportColumn> columns,
        DateTime exportedAt,
        string? language = null)
    {
        var arabic = CustomerExportLabels.IsArabic(language);
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(UnpaidInvoiceExportLabels.SheetName(arabic));

        if (arabic)
            worksheet.RightToLeft = true;

        WriteSummary(worksheet, rows, exportedAt, arabic);
        WriteTable(worksheet, rows, columns, arabic);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void WriteSummary(
        IXLWorksheet worksheet,
        IReadOnlyList<UnpaidInvoiceExportRow> rows,
        DateTime exportedAt,
        bool arabic)
    {
        worksheet.Cell(1, 1).Value = UnpaidInvoiceExportLabels.Title(arabic);
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        worksheet.Cell(2, 1).Value = UnpaidInvoiceExportLabels.InvoiceCount(arabic);
        worksheet.Cell(2, 2).Value = rows.Count;
        worksheet.Cell(3, 1).Value = UnpaidInvoiceExportLabels.OutstandingBalance(arabic);
        worksheet.Cell(3, 2).Value = rows.Sum(row => row.AmountDue);
        worksheet.Cell(3, 2).Style.NumberFormat.Format = MoneyFormat;
        worksheet.Cell(4, 1).Value = CustomerExportLabels.ExportedAt(arabic);
        worksheet.Cell(4, 2).Value = exportedAt;
        worksheet.Cell(4, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";
        worksheet.Range(2, 1, 4, 1).Style.Font.Bold = true;
    }

    private static void WriteTable(
        IXLWorksheet worksheet,
        IReadOnlyList<UnpaidInvoiceExportRow> rows,
        IReadOnlyList<UnpaidInvoiceExportColumn> columns,
        bool arabic)
    {
        const int headerRow = 6;

        for (var index = 0; index < columns.Count; index++)
        {
            var cell = worksheet.Cell(headerRow, index + 1);
            cell.Value = UnpaidInvoiceExportLabels.Header(columns[index], arabic);
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml(HeaderFill);
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        var rowNumber = headerRow + 1;
        foreach (var row in rows)
        {
            for (var index = 0; index < columns.Count; index++)
                WriteCell(worksheet.Cell(rowNumber, index + 1), columns[index], row, arabic);

            rowNumber++;
        }

        worksheet.Cell(rowNumber, 1).Value = UnpaidInvoiceExportLabels.Total(arabic);
        var amountDueIndex = IndexOf(columns, UnpaidInvoiceExportColumn.AmountDue);
        if (amountDueIndex >= 0)
        {
            worksheet.Cell(rowNumber, amountDueIndex + 1).Value = rows.Sum(row => row.AmountDue);
            worksheet.Cell(rowNumber, amountDueIndex + 1).Style.NumberFormat.Format = MoneyFormat;
        }

        worksheet.Range(rowNumber, 1, rowNumber, columns.Count).Style.Font.Bold = true;
        worksheet.Range(rowNumber, 1, rowNumber, columns.Count).Style.Border.TopBorder = XLBorderStyleValues.Thin;

        worksheet.Range(headerRow, 1, rowNumber - 1, columns.Count).SetAutoFilter();
        worksheet.SheetView.FreezeRows(headerRow);
        worksheet.Columns(1, columns.Count).AdjustToContents();
        SetColumnWidth(worksheet, columns, UnpaidInvoiceExportColumn.Address, 24);
        SetColumnWidth(worksheet, columns, UnpaidInvoiceExportColumn.CustomerName, 22);
    }

    private static void SetColumnWidth(
        IXLWorksheet worksheet,
        IReadOnlyList<UnpaidInvoiceExportColumn> columns,
        UnpaidInvoiceExportColumn column,
        double width)
    {
        var index = IndexOf(columns, column);
        if (index >= 0)
            worksheet.Column(index + 1).Width = width;
    }

    private static int IndexOf(
        IReadOnlyList<UnpaidInvoiceExportColumn> columns,
        UnpaidInvoiceExportColumn column)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            if (columns[i] == column)
                return i;
        }

        return -1;
    }

    private static void WriteCell(
        IXLCell cell,
        UnpaidInvoiceExportColumn column,
        UnpaidInvoiceExportRow row,
        bool arabic)
    {
        switch (column)
        {
            case UnpaidInvoiceExportColumn.InvoiceNumber:
                cell.Value = row.InvoiceNumber;
                break;
            case UnpaidInvoiceExportColumn.ConsumptionStart:
                SetDate(cell, row.ConsumptionStart);
                break;
            case UnpaidInvoiceExportColumn.ConsumptionEnd:
                SetDate(cell, row.ConsumptionEnd);
                break;
            case UnpaidInvoiceExportColumn.PaymentDueDate:
                SetDate(cell, row.PaymentDueDate);
                break;
            case UnpaidInvoiceExportColumn.InvoiceStatus:
                SetText(cell, UnpaidInvoiceExportLabels.InvoiceStatus(row.InvoiceStatus, arabic));
                break;
            case UnpaidInvoiceExportColumn.TotalAmount:
                SetMoney(cell, row.TotalAmount);
                break;
            case UnpaidInvoiceExportColumn.PaidAmount:
                SetMoney(cell, row.PaidAmount);
                break;
            case UnpaidInvoiceExportColumn.AmountDue:
                SetMoney(cell, row.AmountDue);
                break;
            case UnpaidInvoiceExportColumn.BilledConsumption:
                if (row.BilledConsumption.HasValue)
                    cell.Value = row.BilledConsumption.Value;
                break;
            case UnpaidInvoiceExportColumn.FixedCharge:
                SetMoney(cell, row.FixedCharge);
                break;
            case UnpaidInvoiceExportColumn.TVA:
                SetMoney(cell, row.TVA);
                break;
            case UnpaidInvoiceExportColumn.CustomerName:
                SetText(cell, row.CustomerName);
                break;
            case UnpaidInvoiceExportColumn.CustomerPhone:
                SetText(cell, row.CustomerPhone);
                break;
            case UnpaidInvoiceExportColumn.Address:
                SetText(cell, row.Address);
                break;
            case UnpaidInvoiceExportColumn.Building:
                SetText(cell, row.Building);
                break;
            case UnpaidInvoiceExportColumn.Floor:
                SetText(cell, row.Floor);
                break;
            case UnpaidInvoiceExportColumn.CableName:
                SetText(cell, row.CableName);
                break;
            case UnpaidInvoiceExportColumn.AreaName:
                SetText(cell, row.AreaName);
                break;
            case UnpaidInvoiceExportColumn.BoxName:
                SetText(cell, row.BoxName);
                break;
            case UnpaidInvoiceExportColumn.AmpereScheduleName:
                SetText(cell, row.AmpereScheduleName);
                break;
            case UnpaidInvoiceExportColumn.CustomerType:
                SetText(cell, CustomerExportLabels.CustomerType(row.CustomerType.ToString(), arabic));
                break;
            case UnpaidInvoiceExportColumn.Plan:
                SetText(cell, CustomerExportLabels.Plan(row.Plan, arabic));
                break;
            case UnpaidInvoiceExportColumn.PlanValue:
                SetMoney(cell, row.PlanValue);
                break;
            case UnpaidInvoiceExportColumn.CustomerStatus:
                SetText(cell, CustomerExportLabels.CustomerStatus(row.CustomerStatus.ToString(), arabic));
                break;
            case UnpaidInvoiceExportColumn.CustomerRelation:
                SetText(cell, CustomerExportLabels.CustomerRelation(row.CustomerRelation?.ToString(), arabic));
                break;
        }
    }

    private static void SetDate(IXLCell cell, DateOnly value)
    {
        cell.Value = value.ToDateTime(TimeOnly.MinValue);
        cell.Style.DateFormat.Format = DateFormat;
    }

    private static void SetMoney(IXLCell cell, decimal value)
    {
        cell.Value = value;
        cell.Style.NumberFormat.Format = MoneyFormat;
    }

    private static void SetText(IXLCell cell, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            cell.Value = value;
    }
}
