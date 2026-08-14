using ClosedXML.Excel;
using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Services.Export
{
    public sealed class ClosedXmlCustomerExportWorkbookBuilder : ICustomerExportWorkbookBuilder
    {
        public ICustomerExportWorkbook Create(
            IReadOnlyList<CustomerExportColumn> columns,
            DateTime exportedAt)
            => new CustomerExportWorkbook(columns, exportedAt);

        private sealed class CustomerExportWorkbook : ICustomerExportWorkbook
        {
            private const string MoneyFormat = "#,##0.00";
            private const string ReadingFormat = "#,##0.00";
            private const string DateFormat = "yyyy-mm-dd";
            private const string BoxFill = "#DDEBF7";
            private const string PlanFill = "#EDF4FB";
            private const string HeaderFill = "#F2F2F2";
            private static readonly char[] InvalidSheetNameChars = ['\\', '/', '?', '*', '[', ']', ':'];

            private readonly XLWorkbook _workbook = new();
            private readonly IReadOnlyList<CustomerExportColumn> _columns;
            private readonly DateTime _exportedAt;
            private readonly HashSet<string> _usedSheetNames = new(StringComparer.OrdinalIgnoreCase);

            public CustomerExportWorkbook(
                IReadOnlyList<CustomerExportColumn> columns,
                DateTime exportedAt)
            {
                _columns = columns;
                _exportedAt = exportedAt;
            }

            public void AddSheet(CustomerExportSheet sheet)
            {
                var worksheet = _workbook.AddWorksheet(UniqueSheetName(sheet.AreaName));
                var row = WriteHeaderBlock(worksheet, sheet);

                if (sheet.Groups.Count == 0)
                {
                    worksheet.Cell(row, 1).Value = "No customers in this area.";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;
                    AdjustColumns(worksheet);
                    return;
                }

                foreach (var group in sheet.Groups)
                {
                    row = WriteGroup(worksheet, group, row);
                    row++;
                }

                if (HasMoneyColumns())
                    WriteTotalRow(worksheet, row, "AREA TOTAL", sheet.Groups.SelectMany(g => g.AllCustomers));

                AdjustColumns(worksheet);
            }

            public void AddStructureSheet(AreaStructureSheet sheet)
            {
                var worksheet = _workbook.AddWorksheet(UniqueSheetName(sheet.AreaName));

                worksheet.Cell(1, 1).Value = "Area:";
                worksheet.Cell(1, 2).Value = sheet.AreaName;
                worksheet.Cell(2, 1).Value = "Boxes:";
                worksheet.Cell(2, 2).Value = sheet.Boxes.Count;
                worksheet.Cell(3, 1).Value = "Exported at:";
                worksheet.Cell(3, 2).Value = _exportedAt;
                worksheet.Cell(3, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";
                worksheet.Range(1, 1, 3, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 2).Style.Font.Bold = true;

                var row = 5;

                if (sheet.Boxes.Count == 0)
                {
                    worksheet.Cell(row, 1).Value = "No boxes in this area.";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;
                    worksheet.Columns(1, 4).AdjustToContents();
                    return;
                }

                var headers = new[] { "Box", "Location Note", "Notes", "Customers" };
                for (var i = 0; i < headers.Length; i++)
                {
                    var headerCell = worksheet.Cell(row, i + 1);
                    headerCell.Value = headers[i];
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml(HeaderFill);
                    headerCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                }
                row++;

                foreach (var box in sheet.Boxes)
                {
                    worksheet.Cell(row, 1).Value = box.Name;
                    SetText(worksheet.Cell(row, 2), box.LocationNote);
                    SetText(worksheet.Cell(row, 3), box.Notes);
                    worksheet.Cell(row, 4).Value = box.CustomerCount;
                    row++;
                }

                worksheet.Cell(row, 1).Value = "TOTAL";
                worksheet.Cell(row, 4).Value = sheet.Boxes.Sum(b => b.CustomerCount);
                var totalRange = worksheet.Range(row, 1, row, 4);
                totalRange.Style.Font.Bold = true;
                totalRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

                worksheet.Columns(1, 4).AdjustToContents();
            }

            public void AddBoxSheet(CustomerExportBoxSheet sheet)
            {
                var worksheet = _workbook.AddWorksheet(UniqueSheetName(sheet.SheetName));
                var row = WriteBoxHeaderBlock(worksheet, sheet.AreaName, sheet.BoxName, sheet.CustomerCount);

                if (sheet.Plans.Count == 0)
                {
                    worksheet.Cell(row, 1).Value = "No customers in this box.";
                    worksheet.Cell(row, 1).Style.Font.Italic = true;
                    AdjustColumns(worksheet);
                    return;
                }

                foreach (var planGroup in sheet.Plans)
                {
                    row = WritePlanSection(worksheet, planGroup, row);
                    row++;
                }

                if (sheet.Plans.Count > 1 && HasMoneyColumns())
                    WriteTotalRow(worksheet, row, "BOX TOTAL", sheet.Plans.SelectMany(p => p.Customers));

                AdjustColumns(worksheet);
            }

            public void AddBoxStructureSheet(BoxStructureSheet sheet)
            {
                var worksheet = _workbook.AddWorksheet(UniqueSheetName(sheet.SheetName));

                worksheet.Cell(1, 1).Value = "Area:";
                worksheet.Cell(1, 2).Value = sheet.AreaName;
                worksheet.Cell(2, 1).Value = "Box:";
                worksheet.Cell(2, 2).Value = sheet.Box.Name;
                worksheet.Cell(3, 1).Value = "Location Note:";
                SetText(worksheet.Cell(3, 2), sheet.Box.LocationNote);
                worksheet.Cell(4, 1).Value = "Notes:";
                SetText(worksheet.Cell(4, 2), sheet.Box.Notes);
                worksheet.Cell(5, 1).Value = "Customers:";
                worksheet.Cell(5, 2).Value = sheet.Box.CustomerCount;
                worksheet.Cell(6, 1).Value = "Exported at:";
                worksheet.Cell(6, 2).Value = _exportedAt;
                worksheet.Cell(6, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";

                worksheet.Range(1, 1, 6, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 2).Style.Font.Bold = true;
                worksheet.Cell(2, 2).Style.Font.Bold = true;

                worksheet.Columns(1, 2).AdjustToContents();
            }

            public byte[] ToBytes()
            {
                if (!_workbook.Worksheets.Any())
                {
                    var empty = _workbook.AddWorksheet("Export");
                    empty.Cell(1, 1).Value = "Nothing matched this export.";
                    empty.Cell(2, 1).Value = "Exported at:";
                    empty.Cell(2, 2).Value = _exportedAt;
                    empty.Cell(2, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";
                }

                using var stream = new MemoryStream();
                _workbook.SaveAs(stream);
                return stream.ToArray();
            }

            public void Dispose() => _workbook.Dispose();

            private int WriteHeaderBlock(IXLWorksheet worksheet, CustomerExportSheet sheet)
            {
                worksheet.Cell(1, 1).Value = "Area:";
                worksheet.Cell(1, 2).Value = sheet.AreaName;
                worksheet.Cell(2, 1).Value = "Customers:";
                worksheet.Cell(2, 2).Value = sheet.CustomerCount;
                worksheet.Cell(3, 1).Value = "Exported at:";
                worksheet.Cell(3, 2).Value = _exportedAt;
                worksheet.Cell(3, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";

                worksheet.Range(1, 1, 3, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 2).Style.Font.Bold = true;

                return 5;
            }

            private int WriteBoxHeaderBlock(
                IXLWorksheet worksheet,
                string areaName,
                string boxName,
                int customerCount)
            {
                worksheet.Cell(1, 1).Value = "Area:";
                worksheet.Cell(1, 2).Value = areaName;
                worksheet.Cell(2, 1).Value = "Box:";
                worksheet.Cell(2, 2).Value = boxName;
                worksheet.Cell(3, 1).Value = "Customers:";
                worksheet.Cell(3, 2).Value = customerCount;
                worksheet.Cell(4, 1).Value = "Exported at:";
                worksheet.Cell(4, 2).Value = _exportedAt;
                worksheet.Cell(4, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";

                worksheet.Range(1, 1, 4, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 2).Style.Font.Bold = true;
                worksheet.Cell(2, 2).Style.Font.Bold = true;

                return 6;
            }

            private int WritePlanSection(
                IXLWorksheet worksheet,
                CustomerExportPlanGroup planGroup,
                int startRow)
            {
                var row = startRow;

                var planCell = worksheet.Cell(row, 1);
                planCell.Value = planGroup.Plan.ToString();
                planCell.Style.Font.Bold = true;
                planCell.Style.Font.Italic = true;
                worksheet.Range(row, 1, row, _columns.Count).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(PlanFill);
                row++;

                WriteColumnHeaders(worksheet, row);
                row++;

                foreach (var customer in planGroup.Customers)
                {
                    for (var i = 0; i < _columns.Count; i++)
                        WriteCell(worksheet.Cell(row, i + 1), _columns[i], customer);

                    row++;
                }

                if (HasMoneyColumns())
                {
                    WriteTotalRow(worksheet, row, $"{planGroup.Plan} Subtotal", planGroup.Customers);
                    row++;
                }

                return row;
            }

            private int WriteGroup(
                IXLWorksheet worksheet,
                CustomerExportGroup group,
                int startRow)
            {
                var row = startRow;

                var titleCell = worksheet.Cell(row, 1);
                titleCell.Value = group.BoxName;
                titleCell.Style.Font.Bold = true;
                worksheet.Range(row, 1, row, _columns.Count).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(BoxFill);
                row++;

                foreach (var planGroup in group.Plans)
                {
                    var planCell = worksheet.Cell(row, 1);
                    planCell.Value = planGroup.Plan.ToString();
                    planCell.Style.Font.Bold = true;
                    planCell.Style.Font.Italic = true;
                    worksheet.Range(row, 1, row, _columns.Count).Style.Fill.BackgroundColor =
                        XLColor.FromHtml(PlanFill);
                    row++;

                    WriteColumnHeaders(worksheet, row);
                    row++;

                    foreach (var customer in planGroup.Customers)
                    {
                        for (var i = 0; i < _columns.Count; i++)
                            WriteCell(worksheet.Cell(row, i + 1), _columns[i], customer);

                        row++;
                    }

                    if (HasMoneyColumns())
                    {
                        WriteTotalRow(worksheet, row, $"{planGroup.Plan} Subtotal", planGroup.Customers);
                        row++;
                    }
                }

                if (group.Plans.Count > 1 && HasMoneyColumns())
                {
                    WriteTotalRow(worksheet, row, $"{group.BoxName} Total", group.AllCustomers);
                    row++;
                }

                return row;
            }

            private void WriteColumnHeaders(IXLWorksheet worksheet, int row)
            {
                for (var i = 0; i < _columns.Count; i++)
                {
                    var headerCell = worksheet.Cell(row, i + 1);
                    headerCell.Value = CustomerExportColumns.Header(_columns[i]);
                    headerCell.Style.Font.Bold = true;
                    headerCell.Style.Fill.BackgroundColor = XLColor.FromHtml(HeaderFill);
                    headerCell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                }
            }

            private void WriteTotalRow(
                IXLWorksheet worksheet,
                int row,
                string label,
                IEnumerable<CustomerExportRow> customers)
            {
                var rows = customers as IReadOnlyList<CustomerExportRow> ?? customers.ToList();

                worksheet.Cell(row, LabelColumn()).Value = label;

                for (var i = 0; i < _columns.Count; i++)
                {
                    var column = _columns[i];
                    if (!CustomerExportColumns.IsMoney(column))
                        continue;

                    var cell = worksheet.Cell(row, i + 1);
                    cell.Value = rows.Sum(r => MoneyValue(column, r));
                    cell.Style.NumberFormat.Format = MoneyFormat;
                }

                var range = worksheet.Range(row, 1, row, Math.Max(_columns.Count, LabelColumn()));
                range.Style.Font.Bold = true;
                range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            }

            private static decimal MoneyValue(CustomerExportColumn column, CustomerExportRow row) => column switch
            {
                CustomerExportColumn.PlanValue => row.PlanValue,
                CustomerExportColumn.TotalBilled => row.TotalBilled,
                CustomerExportColumn.TotalPaid => row.TotalPaid,
                CustomerExportColumn.TotalToPay => row.TotalToPay,
                _ => 0m
            };

            private bool HasMoneyColumns() => _columns.Any(CustomerExportColumns.IsMoney);


            private int LabelColumn()
            {
                for (var i = 0; i < _columns.Count; i++)
                    if (!CustomerExportColumns.IsMoney(_columns[i]))
                        return i + 1;

                return _columns.Count + 1;
            }

            private static void WriteCell(IXLCell cell, CustomerExportColumn column, CustomerExportRow row)
            {
                switch (column)
                {
                    case CustomerExportColumn.Name:
                        cell.Value = row.Name;
                        break;
                    case CustomerExportColumn.Phone:
                        SetText(cell, row.Phone);
                        break;
                    case CustomerExportColumn.Address:
                        SetText(cell, row.Address);
                        break;
                    case CustomerExportColumn.Building:
                        SetText(cell, row.Building);
                        break;
                    case CustomerExportColumn.Floor:
                        SetText(cell, row.Floor);
                        break;
                    case CustomerExportColumn.CableName:
                        SetText(cell, row.CableName);
                        break;
                    case CustomerExportColumn.AreaName:
                        SetText(cell, row.AreaName);
                        break;
                    case CustomerExportColumn.BoxName:
                        SetText(cell, row.BoxName);
                        break;
                    case CustomerExportColumn.AmpereScheduleName:
                        SetText(cell, row.AmpereScheduleName);
                        break;
                    case CustomerExportColumn.CustomerType:
                        SetText(cell, row.CustomerType);
                        break;
                    case CustomerExportColumn.Plan:
                        SetText(cell, row.Plan.ToString());
                        break;
                    case CustomerExportColumn.CustomerStatus:
                        SetText(cell, row.CustomerStatus);
                        break;
                    case CustomerExportColumn.CustomerRelation:
                        SetText(cell, row.CustomerRelation);
                        break;
                    case CustomerExportColumn.SubscriptionDate:
                        cell.Value = row.SubscriptionDate.ToDateTime(TimeOnly.MinValue);
                        cell.Style.DateFormat.Format = DateFormat;
                        break;
                    case CustomerExportColumn.InitialMeterReading:
                        SetNumber(cell, row.InitialMeterReading, ReadingFormat);
                        break;
                    case CustomerExportColumn.LatestMeterReading:
                        SetNumber(cell, row.LatestMeterReading, ReadingFormat);
                        break;
                    case CustomerExportColumn.PlanValue:
                        SetNumber(cell, row.PlanValue, MoneyFormat);
                        break;
                    case CustomerExportColumn.TotalBilled:
                        SetNumber(cell, row.TotalBilled, MoneyFormat);
                        break;
                    case CustomerExportColumn.TotalPaid:
                        SetNumber(cell, row.TotalPaid, MoneyFormat);
                        break;
                    case CustomerExportColumn.TotalToPay:
                        SetNumber(cell, row.TotalToPay, MoneyFormat);
                        break;
                }
            }

            private static void SetText(IXLCell cell, string? value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    cell.Value = value;
            }

            private static void SetNumber(IXLCell cell, decimal? value, string format)
            {
                if (!value.HasValue)
                    return;

                cell.Value = value.Value;
                cell.Style.NumberFormat.Format = format;
            }

            private void AdjustColumns(IXLWorksheet worksheet)
                => worksheet.Columns(1, Math.Max(_columns.Count + 1, 2)).AdjustToContents();


            private string UniqueSheetName(string name)
            {
                var cleaned = new string(name
                    .Where(c => !InvalidSheetNameChars.Contains(c))
                    .ToArray())
                    .Trim();

                if (string.IsNullOrWhiteSpace(cleaned))
                    cleaned = "Sheet";

                if (cleaned.Length > 31)
                    cleaned = cleaned[..31];

                var candidate = cleaned;
                var suffix = 2;

                while (!_usedSheetNames.Add(candidate))
                {
                    var tail = $" ({suffix++})";
                    var head = cleaned.Length + tail.Length > 31
                        ? cleaned[..(31 - tail.Length)]
                        : cleaned;
                    candidate = head + tail;
                }

                return candidate;
            }
        }
    }
}

