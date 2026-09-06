using ClosedXML.Excel;
using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Services.Export;

public sealed class ClosedXmlCustomerExportWorkbookBuilder : ICustomerExportWorkbookBuilder
{
    public ICustomerExportWorkbook Create(
        IReadOnlyList<CustomerExportColumn> columns,
        DateTime exportedAt,
        string? language = null)
        => new CustomerExportWorkbook(columns, exportedAt, CustomerExportLabels.IsArabic(language));

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
        private readonly bool _arabic;
        private readonly HashSet<string> _usedSheetNames = new(StringComparer.OrdinalIgnoreCase);

        public CustomerExportWorkbook(
            IReadOnlyList<CustomerExportColumn> columns,
            DateTime exportedAt,
            bool arabic)
        {
            _columns = columns;
            _exportedAt = exportedAt;
            _arabic = arabic;
        }

        public void AddSheet(CustomerExportSheet sheet)
        {
            var worksheet = AddLocalizedWorksheet(sheet.AreaName);
            var row = WriteHeaderBlock(worksheet, sheet);

            if (sheet.Groups.Count == 0)
            {
                worksheet.Cell(row, 1).Value = CustomerExportLabels.NoCustomersInArea(_arabic);
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
                WriteTotalRow(
                    worksheet,
                    row,
                    CustomerExportLabels.AreaTotal(_arabic),
                    sheet.Groups.SelectMany(g => g.AllCustomers));

            AdjustColumns(worksheet);
        }

        public void AddFlatSheet(string sheetName, IReadOnlyList<CustomerExportRow> rows)
        {
            var worksheet = AddLocalizedWorksheet(sheetName);

            worksheet.Cell(1, 1).Value = CustomerExportLabels.StackedTitle(_arabic);
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 14;
            worksheet.Cell(2, 1).Value = CustomerExportLabels.Customers(_arabic);
            worksheet.Cell(2, 2).Value = rows.Count;
            worksheet.Cell(3, 1).Value = CustomerExportLabels.ExportedAt(_arabic);
            worksheet.Cell(3, 2).Value = _exportedAt;
            worksheet.Cell(3, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";
            worksheet.Range(2, 1, 3, 1).Style.Font.Bold = true;

            const int headerRow = 5;
            WriteColumnHeaders(worksheet, headerRow);

            var row = headerRow + 1;
            foreach (var customer in rows)
            {
                for (var i = 0; i < _columns.Count; i++)
                    WriteCell(worksheet.Cell(row, i + 1), _columns[i], customer);

                row++;
            }

            if (rows.Count > 0 && HasMoneyColumns())
                WriteTotalRow(worksheet, row, CustomerExportLabels.Total(_arabic), rows);

            if (rows.Count > 0)
            {
                worksheet.Range(headerRow, 1, row - 1, _columns.Count).SetAutoFilter();
                worksheet.SheetView.FreezeRows(headerRow);
            }

            AdjustColumns(worksheet);
        }

        public void AddStructureSheet(AreaStructureSheet sheet)
        {
            var worksheet = AddLocalizedWorksheet(sheet.AreaName);

            worksheet.Cell(1, 1).Value = CustomerExportLabels.Area(_arabic);
            worksheet.Cell(1, 2).Value = sheet.AreaName;
            worksheet.Cell(2, 1).Value = CustomerExportLabels.Boxes(_arabic);
            worksheet.Cell(2, 2).Value = sheet.Boxes.Count;
            worksheet.Cell(3, 1).Value = CustomerExportLabels.ExportedAt(_arabic);
            worksheet.Cell(3, 2).Value = _exportedAt;
            worksheet.Cell(3, 2).Style.DateFormat.Format = $"{DateFormat} hh:mm";
            worksheet.Range(1, 1, 3, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 2).Style.Font.Bold = true;

            var row = 5;

            if (sheet.Boxes.Count == 0)
            {
                worksheet.Cell(row, 1).Value = CustomerExportLabels.NoBoxesInArea(_arabic);
                worksheet.Cell(row, 1).Style.Font.Italic = true;
                worksheet.Columns(1, 4).AdjustToContents();
                return;
            }

            var headers = new[]
            {
                CustomerExportLabels.StructureBox(_arabic),
                CustomerExportLabels.StructureLocationNote(_arabic),
                CustomerExportLabels.StructureNotes(_arabic),
                CustomerExportLabels.StructureCustomers(_arabic)
            };
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

            worksheet.Cell(row, 1).Value = CustomerExportLabels.Total(_arabic);
            worksheet.Cell(row, 4).Value = sheet.Boxes.Sum(b => b.CustomerCount);
            var totalRange = worksheet.Range(row, 1, row, 4);
            totalRange.Style.Font.Bold = true;
            totalRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;

            worksheet.Columns(1, 4).AdjustToContents();
        }

        public void AddBoxSheet(CustomerExportBoxSheet sheet)
        {
            var worksheet = AddLocalizedWorksheet(sheet.SheetName);
            var row = WriteBoxHeaderBlock(worksheet, sheet.AreaName, sheet.BoxName, sheet.CustomerCount);

            if (sheet.Plans.Count == 0)
            {
                worksheet.Cell(row, 1).Value = CustomerExportLabels.NoCustomersInBox(_arabic);
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
                WriteTotalRow(
                    worksheet,
                    row,
                    CustomerExportLabels.BoxTotal(_arabic),
                    sheet.Plans.SelectMany(p => p.Customers));

            AdjustColumns(worksheet);
        }

        public void AddBoxStructureSheet(BoxStructureSheet sheet)
        {
            var worksheet = AddLocalizedWorksheet(sheet.SheetName);

            worksheet.Cell(1, 1).Value = CustomerExportLabels.Area(_arabic);
            worksheet.Cell(1, 2).Value = sheet.AreaName;
            worksheet.Cell(2, 1).Value = CustomerExportLabels.Box(_arabic);
            worksheet.Cell(2, 2).Value = sheet.Box.Name;
            worksheet.Cell(3, 1).Value = CustomerExportLabels.LocationNote(_arabic);
            SetText(worksheet.Cell(3, 2), sheet.Box.LocationNote);
            worksheet.Cell(4, 1).Value = CustomerExportLabels.Notes(_arabic);
            SetText(worksheet.Cell(4, 2), sheet.Box.Notes);
            worksheet.Cell(5, 1).Value = CustomerExportLabels.Customers(_arabic);
            worksheet.Cell(5, 2).Value = sheet.Box.CustomerCount;
            worksheet.Cell(6, 1).Value = CustomerExportLabels.ExportedAt(_arabic);
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
                var empty = AddLocalizedWorksheet(CustomerExportLabels.EmptySheetName(_arabic));
                empty.Cell(1, 1).Value = CustomerExportLabels.NothingMatched(_arabic);
                empty.Cell(2, 1).Value = CustomerExportLabels.ExportedAt(_arabic);
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
            worksheet.Cell(1, 1).Value = CustomerExportLabels.Area(_arabic);
            worksheet.Cell(1, 2).Value = sheet.AreaName;
            worksheet.Cell(2, 1).Value = CustomerExportLabels.Customers(_arabic);
            worksheet.Cell(2, 2).Value = sheet.CustomerCount;
            worksheet.Cell(3, 1).Value = CustomerExportLabels.ExportedAt(_arabic);
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
            worksheet.Cell(1, 1).Value = CustomerExportLabels.Area(_arabic);
            worksheet.Cell(1, 2).Value = areaName;
            worksheet.Cell(2, 1).Value = CustomerExportLabels.Box(_arabic);
            worksheet.Cell(2, 2).Value = boxName;
            worksheet.Cell(3, 1).Value = CustomerExportLabels.Customers(_arabic);
            worksheet.Cell(3, 2).Value = customerCount;
            worksheet.Cell(4, 1).Value = CustomerExportLabels.ExportedAt(_arabic);
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
            planCell.Value = CustomerExportLabels.Plan(planGroup.Plan, _arabic);
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
                WriteTotalRow(worksheet, row, CustomerExportLabels.PlanSubtotal(planGroup.Plan, _arabic), planGroup.Customers);
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
                planCell.Value = CustomerExportLabels.Plan(planGroup.Plan, _arabic);
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
                    WriteTotalRow(worksheet, row, CustomerExportLabels.PlanSubtotal(planGroup.Plan, _arabic), planGroup.Customers);
                    row++;
                }
            }

            if (group.Plans.Count > 1 && HasMoneyColumns())
            {
                WriteTotalRow(worksheet, row, CustomerExportLabels.NamedTotal(group.BoxName, _arabic), group.AllCustomers);
                row++;
            }

            return row;
        }

        private void WriteColumnHeaders(IXLWorksheet worksheet, int row)
        {
            for (var i = 0; i < _columns.Count; i++)
            {
                var headerCell = worksheet.Cell(row, i + 1);
                headerCell.Value = CustomerExportLabels.Header(_columns[i], _arabic);
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

        private void WriteCell(IXLCell cell, CustomerExportColumn column, CustomerExportRow row)
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
                    SetText(cell, CustomerExportLabels.CustomerType(row.CustomerType, _arabic));
                    break;
                case CustomerExportColumn.Plan:
                    SetText(cell, CustomerExportLabels.Plan(row.Plan, _arabic));
                    break;
                case CustomerExportColumn.CustomerStatus:
                    SetText(cell, CustomerExportLabels.CustomerStatus(row.CustomerStatus, _arabic));
                    break;
                case CustomerExportColumn.CustomerRelation:
                    SetText(cell, CustomerExportLabels.CustomerRelation(row.CustomerRelation, _arabic));
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

        private IXLWorksheet AddLocalizedWorksheet(string name)
        {
            var worksheet = _workbook.AddWorksheet(UniqueSheetName(name));
            if (_arabic)
                worksheet.RightToLeft = true;
            return worksheet;
        }

        private string UniqueSheetName(string name)
        {
            var cleaned = new string(name
                .Where(c => !InvalidSheetNameChars.Contains(c))
                .ToArray())
                .Trim();

            if (string.IsNullOrWhiteSpace(cleaned))
                cleaned = CustomerExportLabels.EmptySheetName(_arabic);

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
