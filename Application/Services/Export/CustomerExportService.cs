using Shabakat.Application.Contracts.Abstractions;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Exports;
using Shabakat.Application.Helper;
using Shabakat.Application.Mappers;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Export;

public sealed class CustomerExportService : ICustomerExportService
{
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string UnassignedAreaName = "Unassigned";
    private const string NoBoxGroupName = "No Box";

    private readonly ICustomerExportRepository _exportRepository;
    private readonly ICustomerExportWorkbookBuilder _workbookBuilder;
    private readonly IAppPreferencesRepository _preferencesRepository;

    public CustomerExportService(
        ICustomerExportRepository exportRepository,
        ICustomerExportWorkbookBuilder workbookBuilder,
        IAppPreferencesRepository preferencesRepository)
    {
        _exportRepository = exportRepository;
        _workbookBuilder = workbookBuilder;
        _preferencesRepository = preferencesRepository;
    }

    public async Task<IReadOnlyList<CustomerExportColumn>> GetSelectedColumnsAsync(
        CancellationToken cancellationToken = default)
    {
        var prefs = await _preferencesRepository.GetAsync();
        if (prefs?.CustomerExportColumnPreference is null)
            return CustomerExportColumns.Default;

        return prefs.CustomerExportColumnPreference.ToSelectedColumns();
    }

    public async Task SaveSelectedColumnsAsync(
        IReadOnlyCollection<CustomerExportColumn> columns,
        CancellationToken cancellationToken = default)
    {
        var selected = columns.Distinct().ToList();
        if (selected.Count == 0)
            throw new DomainException("Select at least one export column.");

        var prefs = await _preferencesRepository.GetAsync();
        if (prefs is null)
        {
            await _preferencesRepository.AddAsync(new AppPreferences
            {
                CustomerExportColumnPreference = new CustomerExportColumnPreference().Apply(selected)
            });
        }
        else
        {
            prefs.EnsureExportColumns().Apply(selected);
            _preferencesRepository.Update(prefs);
        }

        await _preferencesRepository.SaveChangesAsync();
    }

    public async Task<CustomerExportFile> BuildAsync(
        CustomerExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await ResolvePlanAsync(request, cancellationToken);

        var exportedAt = DateTime.Now;
        using var workbook = _workbookBuilder.Create(plan.Columns, exportedAt);

        foreach (var area in plan.Areas)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddAreaExportAsync(workbook, area, plan, cancellationToken);
        }

        if (plan.IncludeUnassigned)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await AddUnassignedSheetAsync(workbook, cancellationToken);
        }

        return BuildFile(workbook, plan, request, exportedAt);
    }

    private async Task<ExportPlan> ResolvePlanAsync(
        CustomerExportRequest request,
        CancellationToken cancellationToken)
    {
        var areas = await _exportRepository.GetAreasAsync(request.AreaIds, cancellationToken);
        var structureOnly = request.Scope == CustomerExportScope.AreasAndBoxes;
        var singleAreaPerBox = IsSingleAreaExport(request, areas);

        var includeUnassigned = !structureOnly
            && !singleAreaPerBox
            && (request.AreaIds is null || request.AreaIds.Count == 0);

        var columns = request.Columns is { Count: > 0 }
            ? CustomerExportColumns.Resolve(request.Columns)
            : await GetSelectedColumnsAsync(cancellationToken);

        return new ExportPlan(
            columns,
            areas,
            structureOnly,
            includeUnassigned,
            singleAreaPerBox);
    }

    private async Task AddAreaExportAsync(
        ICustomerExportWorkbook workbook,
        ExportAreaRef area,
        ExportPlan plan,
        CancellationToken cancellationToken)
    {
        if (plan.SingleAreaPerBoxSheets)
        {
            await foreach (var _ in AddSingleAreaPerBoxSheetsAsync(
                workbook, area, plan.StructureOnly, cancellationToken))
            {
            }

            return;
        }

        await AddAreaSheetAsync(workbook, area, plan.StructureOnly, cancellationToken);
    }

    private async IAsyncEnumerable<int> AddSingleAreaPerBoxSheetsAsync(
        ICustomerExportWorkbook workbook,
        ExportAreaRef area,
        bool structureOnly,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (structureOnly)
        {
            var boxes = await _exportRepository.GetBoxesForAreaAsync(area.Id, cancellationToken);

            if (boxes.Count == 0)
            {
                workbook.AddStructureSheet(new AreaStructureSheet(area.Name, []));
                yield return 0;
                yield break;
            }

            foreach (var box in boxes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                workbook.AddBoxStructureSheet(new BoxStructureSheet(box.Name, area.Name, box));
                yield return 0;
            }

            yield break;
        }

        var rows = await _exportRepository.GetRowsForAreaAsync(area.Id, cancellationToken);
        var groups = BuildGroups(rows);

        if (groups.Count == 0)
        {
            workbook.AddBoxSheet(new CustomerExportBoxSheet(
                area.Name,
                area.Name,
                NoBoxGroupName,
                []));
            yield return 0;
            yield break;
        }

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            workbook.AddBoxSheet(new CustomerExportBoxSheet(
                group.BoxName,
                area.Name,
                group.BoxName,
                group.Plans));
            yield return group.CustomerCount;
        }
    }

    private async Task AddAreaSheetAsync(
        ICustomerExportWorkbook workbook,
        ExportAreaRef area,
        bool structureOnly,
        CancellationToken cancellationToken)
    {
        if (structureOnly)
        {
            var boxes = await _exportRepository.GetBoxesForAreaAsync(area.Id, cancellationToken);
            workbook.AddStructureSheet(new AreaStructureSheet(area.Name, boxes));
            return;
        }

        var rows = await _exportRepository.GetRowsForAreaAsync(area.Id, cancellationToken);
        workbook.AddSheet(BuildSheet(area.Name, rows));
    }

    private async Task AddUnassignedSheetAsync(
        ICustomerExportWorkbook workbook,
        CancellationToken cancellationToken)
    {
        var rows = await _exportRepository.GetRowsWithoutAreaAsync(cancellationToken);
        if (rows.Count > 0)
            workbook.AddSheet(BuildSheet(UnassignedAreaName, rows));
    }

    private static CustomerExportFile BuildFile(
        ICustomerExportWorkbook workbook,
        ExportPlan plan,
        CustomerExportRequest request,
        DateTime exportedAt)
        => new(
            workbook.ToBytes(),
            BuildFileName(plan.Areas, request, exportedAt),
            XlsxContentType);

    private sealed record ExportPlan(
        IReadOnlyList<CustomerExportColumn> Columns,
        IReadOnlyList<ExportAreaRef> Areas,
        bool StructureOnly,
        bool IncludeUnassigned,
        bool SingleAreaPerBoxSheets);

    private static bool IsSingleAreaExport(
        CustomerExportRequest request,
        IReadOnlyList<ExportAreaRef> areas)
        => request.AreaIds is { Count: 1 } && areas.Count == 1;

    private static CustomerExportSheet BuildSheet(
        string areaName,
        IReadOnlyList<CustomerExportRow> rows)
        => new(areaName, BuildGroups(rows));

    private static IReadOnlyList<CustomerExportGroup> BuildGroups(
        IReadOnlyList<CustomerExportRow> rows)
        => rows
            .GroupBy(r => string.IsNullOrWhiteSpace(r.BoxName) ? null : r.BoxName)
            .OrderBy(g => g.Key is null)
            .ThenBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new CustomerExportGroup(
                g.Key ?? NoBoxGroupName,
                BuildPlanGroups(g)))
            .ToList();

    private static IReadOnlyList<CustomerExportPlanGroup> BuildPlanGroups(
        IEnumerable<CustomerExportRow> rows)
        => rows
            .GroupBy(r => r.Plan)
            .OrderBy(g => g.Key)
            .Select(g => new CustomerExportPlanGroup(g.Key, g.ToList()))
            .ToList();

    private static string BuildFileName(
        IReadOnlyList<ExportAreaRef> areas,
        CustomerExportRequest request,
        DateTime exportedAt)
    {
        var isSingleArea = request.AreaIds is { Count: 1 } && areas.Count == 1;
        var label = isSingleArea ? Slugify(areas[0].Name) : "all-areas";
        var prefix = request.Scope == CustomerExportScope.AreasAndBoxes
            ? "areas-boxes"
            : "customers";

        return $"{prefix}-{label}-{exportedAt:yyyyMMdd-HHmm}.xlsx";
    }

    private static string Slugify(string value)
    {
        var cleaned = new string(value
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .ToArray())
            .Trim('-');

        while (cleaned.Contains("--", StringComparison.Ordinal))
            cleaned = cleaned.Replace("--", "-", StringComparison.Ordinal);

        return string.IsNullOrWhiteSpace(cleaned) ? "area" : cleaned.ToLowerInvariant();
    }
}
