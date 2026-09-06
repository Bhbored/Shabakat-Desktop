using Shabakat.Application.Helper;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.DTOs.Exports;

public static class UnpaidInvoiceExportLabels
{
    public static string SheetName(bool arabic) => arabic ? "الفواتير غير المسددة" : "Unpaid invoices";

    public static string Title(bool arabic) => arabic ? "تقرير الفواتير غير المسددة" : "Unpaid invoices report";

    public static string InvoiceCount(bool arabic) => arabic ? "عدد الفواتير:" : "Invoice count:";

    public static string OutstandingBalance(bool arabic) => arabic ? "إجمالي المستحق:" : "Outstanding balance:";

    public static string Total(bool arabic) => arabic ? "الإجمالي" : "TOTAL";

    public static string Header(UnpaidInvoiceExportColumn column, bool arabic) => arabic
        ? ArabicHeader(column)
        : UnpaidInvoiceExportColumns.Header(column);

    public static string InvoiceStatus(string status, bool arabic) => (status, arabic) switch
    {
        ("Unpaid", true) => "غير مسددة",
        ("PartiallyPaid", true) => "مسددة جزئياً",
        ("Paid", true) => "مسددة",
        _ => status
    };

    private static string ArabicHeader(UnpaidInvoiceExportColumn column) => column switch
    {
        UnpaidInvoiceExportColumn.InvoiceNumber => "رقم الفاتورة",
        UnpaidInvoiceExportColumn.ConsumptionStart => "بداية الاستهلاك",
        UnpaidInvoiceExportColumn.ConsumptionEnd => "نهاية الاستهلاك",
        UnpaidInvoiceExportColumn.PaymentDueDate => "تاريخ الاستحقاق",
        UnpaidInvoiceExportColumn.InvoiceStatus => "حالة الفاتورة",
        UnpaidInvoiceExportColumn.TotalAmount => "إجمالي الفاتورة",
        UnpaidInvoiceExportColumn.PaidAmount => "المدفوع",
        UnpaidInvoiceExportColumn.AmountDue => "المستحق",
        UnpaidInvoiceExportColumn.BilledConsumption => "الاستهلاك المفوتر",
        UnpaidInvoiceExportColumn.FixedCharge => "الرسم الثابت",
        UnpaidInvoiceExportColumn.TVA => "TVA",
        UnpaidInvoiceExportColumn.CustomerName => "اسم الزبون",
        UnpaidInvoiceExportColumn.CustomerPhone => "الهاتف",
        UnpaidInvoiceExportColumn.Address => "العنوان",
        UnpaidInvoiceExportColumn.Building => "المبنى",
        UnpaidInvoiceExportColumn.Floor => "الطابق",
        UnpaidInvoiceExportColumn.CableName => "اسم الكابل",
        UnpaidInvoiceExportColumn.AreaName => "المنطقة",
        UnpaidInvoiceExportColumn.BoxName => "العلبة",
        UnpaidInvoiceExportColumn.AmpereScheduleName => "جدول الأمبير",
        UnpaidInvoiceExportColumn.CustomerType => "نوع الزبون",
        UnpaidInvoiceExportColumn.Plan => "الاشتراك",
        UnpaidInvoiceExportColumn.PlanValue => "قيمة الاشتراك",
        UnpaidInvoiceExportColumn.CustomerStatus => "حالة الزبون",
        UnpaidInvoiceExportColumn.CustomerRelation => "العلاقة",
        _ => UnpaidInvoiceExportColumns.Header(column)
    };
}
