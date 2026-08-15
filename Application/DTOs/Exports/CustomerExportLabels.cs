using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Exports;

public static class CustomerExportLabels
{
    public static bool IsArabic(string? language)
        => string.Equals(language?.Trim(), "ar", StringComparison.OrdinalIgnoreCase);

    public static string Header(CustomerExportColumn column, bool arabic) => arabic
        ? ArabicHeader(column)
        : CustomerExportColumns.Header(column);

    public static string Plan(PlanType plan, bool arabic) => (plan, arabic) switch
    {
        (PlanType.Ampere, true) => "أمبير",
        (PlanType.Kilowatt, true) => "كيلوواط",
        (PlanType.FixedKilowatt, true) => "كيلوواط ثابت",
        _ => plan.ToString()
    };

    public static string CustomerType(string? value, bool arabic)
    {
        if (!arabic || string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;

        return value switch
        {
            "Residential" => "سكني",
            "Commercial" => "تجاري",
            "Industrial" => "صناعي",
            _ => value
        };
    }

    public static string CustomerStatus(string? value, bool arabic)
    {
        if (!arabic || string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;

        return value switch
        {
            "Active" => "فعال",
            "Suspended" => "موقوف",
            "Terminated" => "ملغى",
            _ => value
        };
    }

    public static string CustomerRelation(string? value, bool arabic)
    {
        if (!arabic || string.IsNullOrWhiteSpace(value))
            return value ?? string.Empty;

        return value switch
        {
            "Friend" => "صديق",
            "Family" => "عائلة",
            "Owner" => "مالك",
            _ => value
        };
    }

    public static string NoBox(bool arabic) => arabic ? "بدون علبة" : "No Box";

    public static string Unassigned(bool arabic) => arabic ? "غير معيّن" : "Unassigned";

    public static string Area(bool arabic) => arabic ? "المنطقة:" : "Area:";

    public static string Box(bool arabic) => arabic ? "العلبة:" : "Box:";

    public static string Boxes(bool arabic) => arabic ? "العلب:" : "Boxes:";

    public static string Customers(bool arabic) => arabic ? "الزبائن:" : "Customers:";

    public static string ExportedAt(bool arabic) => arabic ? "تاريخ التصدير:" : "Exported at:";

    public static string LocationNote(bool arabic) => arabic ? "ملاحظة الموقع:" : "Location Note:";

    public static string Notes(bool arabic) => arabic ? "ملاحظات:" : "Notes:";

    public static string StructureBox(bool arabic) => arabic ? "العلبة" : "Box";

    public static string StructureLocationNote(bool arabic) => arabic ? "ملاحظة الموقع" : "Location Note";

    public static string StructureNotes(bool arabic) => arabic ? "ملاحظات" : "Notes";

    public static string StructureCustomers(bool arabic) => arabic ? "الزبائن" : "Customers";

    public static string AreaTotal(bool arabic) => arabic ? "إجمالي المنطقة" : "AREA TOTAL";

    public static string BoxTotal(bool arabic) => arabic ? "إجمالي العلبة" : "BOX TOTAL";

    public static string Total(bool arabic) => arabic ? "الإجمالي" : "TOTAL";

    public static string PlanSubtotal(PlanType plan, bool arabic)
        => arabic
            ? $"المجموع الفرعي ({Plan(plan, true)})"
            : $"{plan} Subtotal";

    public static string NamedTotal(string name, bool arabic)
        => arabic ? $"إجمالي {name}" : $"{name} Total";

    public static string NoCustomersInArea(bool arabic)
        => arabic ? "لا يوجد زبائن في هذه المنطقة." : "No customers in this area.";

    public static string NoBoxesInArea(bool arabic)
        => arabic ? "لا توجد علب في هذه المنطقة." : "No boxes in this area.";

    public static string NoCustomersInBox(bool arabic)
        => arabic ? "لا يوجد زبائن في هذه العلبة." : "No customers in this box.";

    public static string NothingMatched(bool arabic)
        => arabic ? "لا توجد بيانات مطابقة لهذا التصدير." : "Nothing matched this export.";

    public static string EmptySheetName(bool arabic) => arabic ? "تصدير" : "Export";

    private static string ArabicHeader(CustomerExportColumn column) => column switch
    {
        CustomerExportColumn.Name => "الاسم",
        CustomerExportColumn.Phone => "الهاتف",
        CustomerExportColumn.Address => "العنوان",
        CustomerExportColumn.Building => "المبنى",
        CustomerExportColumn.Floor => "الطابق",
        CustomerExportColumn.CableName => "اسم الكابل",
        CustomerExportColumn.AreaName => "المنطقة",
        CustomerExportColumn.BoxName => "العلبة",
        CustomerExportColumn.AmpereScheduleName => "جدول الأمبير",
        CustomerExportColumn.CustomerType => "نوع الزبون",
        CustomerExportColumn.Plan => "الاشتراك",
        CustomerExportColumn.PlanValue => "قيمة الاشتراك",
        CustomerExportColumn.SubscriptionDate => "تاريخ الاشتراك",
        CustomerExportColumn.CustomerStatus => "الحالة",
        CustomerExportColumn.CustomerRelation => "العلاقة",
        CustomerExportColumn.InitialMeterReading => "القراءة الأولية",
        CustomerExportColumn.LatestMeterReading => "آخر قراءة",
        CustomerExportColumn.TotalBilled => "إجمالي الفوترة",
        CustomerExportColumn.TotalPaid => "إجمالي المدفوع",
        CustomerExportColumn.TotalToPay => "المستحق",
        _ => CustomerExportColumns.Header(column)
    };
}
