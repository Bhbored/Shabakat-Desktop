namespace Shabakat.Components.Shared;

public record AreaData(string Id, string Name, int CustomerCount, DateTime CreatedAt);
public record BoxData(string Id, string Name, string AreaId, string AreaName, string? LocationNote, string? Notes, int CustomerCount, DateTime CreatedAt);
public record SubscriberData(string Id, string Name, string? Phone, string? Area, string Plan, double PlanValue, DateTime SubscriptionDate, string Status, double AmountDue);
public record InvoiceData(string Id, int InvoiceNumber, string CustomerName, string? Area, double TotalAmount, double PaidAmount, double AmountDue, string Status, DateTime IssueDate, DateTime? DueDate, DateTime CreatedAt);
public record ExpenseData(string Id, string Description, string Category, double Amount, string? AreaName, DateTime Date, string? Notes);
public record AmpereScheduleData(string Id, string Name, double Price, double FixedCharge, double Tva, int SortOrder);
public record NavigationItem(string Href, string Label);

public static class MockData
{
    public static readonly AreaData[] Areas = new[]
    {
        new AreaData("a1", "Hamra", 245, new DateTime(2023, 1, 15)),
        new AreaData("a2", "Achrafieh", 312, new DateTime(2023, 2, 1)),
        new AreaData("a3", "Verdun", 198, new DateTime(2023, 3, 10)),
        new AreaData("a4", "Gemmayzeh", 167, new DateTime(2023, 4, 5)),
        new AreaData("a5", "Mar Elias", 143, new DateTime(2023, 5, 20)),
        new AreaData("a6", "Badaro", 98, new DateTime(2023, 6, 12)),
        new AreaData("a7", "Sodeco", 52, new DateTime(2023, 7, 8)),
        new AreaData("a8", "Ras Beirut", 30, new DateTime(2023, 8, 1)),
        new AreaData("a9", "Raouche", 75, new DateTime(2023, 9, 14)),
        new AreaData("a10", "Tallet el Khayat", 88, new DateTime(2023, 10, 3)),
        new AreaData("a11", "Monot", 45, new DateTime(2023, 11, 22)),
    };

    public static readonly BoxData[] Boxes = new[]
    {
        new BoxData("b1", "BX-001 Bliss Street", "a1", "Hamra", "Bliss Street", "Main distribution box", 45, new DateTime(2023, 3, 1)),
        new BoxData("b2", "BX-002 Sassine Square", "a2", "Achrafieh", "Sassine Square", null, 62, new DateTime(2023, 3, 15)),
        new BoxData("b3", "BX-003 Rachid Karame", "a3", "Verdun", "Rachid Karame St", "Near the mosque", 38, new DateTime(2023, 4, 10)),
        new BoxData("b4", "BX-004 Gouraud Street", "a4", "Gemmayzeh", "Gouraud Street", null, 33, new DateTime(2023, 4, 20)),
        new BoxData("b5", "BX-005 Main Road", "a5", "Mar Elias", "Main Road", "Needs maintenance", 28, new DateTime(2023, 5, 5)),
        new BoxData("b6", "BX-006 Samir Kassir", "a6", "Badaro", "Samir Kassir St", null, 22, new DateTime(2023, 5, 18)),
        new BoxData("b7", "BX-007 Jeanne d'Arc", "a1", "Hamra", "Jeanne d'Arc St", null, 40, new DateTime(2023, 6, 2)),
        new BoxData("b8", "BX-008 Damascus Road", "a7", "Sodeco", "Damascus Road", null, 15, new DateTime(2023, 6, 25)),
    };

    public static readonly AmpereScheduleData[] AmpereSchedules = new[]
    {
        new AmpereScheduleData("s1", "5A", 85, 15, 11, 1),
        new AmpereScheduleData("s2", "10A", 150, 15, 11, 2),
        new AmpereScheduleData("s3", "15A", 210, 20, 11, 3),
        new AmpereScheduleData("s4", "20A", 280, 20, 11, 4),
        new AmpereScheduleData("s5", "30A", 390, 25, 11, 5),
        new AmpereScheduleData("s6", "40A", 510, 30, 11, 6),
        new AmpereScheduleData("s7", "50A", 630, 30, 11, 7),
    };

    public static readonly SubscriberData[] Subscribers = new[]
    {
        new SubscriberData("c1", "Ahmad Khalil", "+961 71 234 567", "Hamra", "Ampere", 5, new DateTime(2024, 1, 15), "paid", 0),
        new SubscriberData("c2", "Rania Mansour", "+961 70 345 678", "Achrafieh", "Ampere", 10, new DateTime(2024, 2, 1), "unpaid", 150),
        new SubscriberData("c3", "Khalid Barakat", "+961 76 456 789", "Verdun", "Ampere", 5, new DateTime(2023, 11, 20), "paid", 0),
        new SubscriberData("c4", "Lara Haddad", "+961 78 567 890", "Gemmayzeh", "Ampere", 15, new DateTime(2024, 3, 10), "overdue", 210),
        new SubscriberData("c5", "Hassan Nassar", "+961 71 678 901", "Mar Elias", "Ampere", 5, new DateTime(2024, 1, 5), "paid", 0),
        new SubscriberData("c6", "Nadia Rizk", "+961 70 789 012", "Badaro", "Ampere", 10, new DateTime(2023, 12, 15), "unpaid", 150),
        new SubscriberData("c7", "Fadi Gemayel", "+961 76 890 123", "Sodeco", "Kilowatt", 20, new DateTime(2024, 4, 1), "paid", 0),
        new SubscriberData("c8", "Carla Khoury", "+961 78 901 234", "Ras Beirut", "Ampere", 5, new DateTime(2024, 2, 20), "paid", 0),
        new SubscriberData("c9", "Rami Assaf", "+961 71 012 345", "Raouche", "Ampere", 10, new DateTime(2023, 10, 1), "overdue", 150),
        new SubscriberData("c10", "Maya Frem", "+961 70 123 456", "Tallet el Khayat", "Ampere", 15, new DateTime(2024, 3, 25), "paid", 0),
        new SubscriberData("c11", "Elie Saab", "+961 76 234 567", "Monot", "Ampere", 5, new DateTime(2024, 1, 30), "unpaid", 85),
        new SubscriberData("c12", "Joelle Abou Jaoude", "+961 78 345 678", "Hamra", "Kilowatt", 20, new DateTime(2023, 9, 15), "paid", 0),
        new SubscriberData("c13", "Georges Nassar", "+961 71 456 789", "Achrafieh", "Ampere", 10, new DateTime(2024, 4, 10), "paid", 0),
        new SubscriberData("c14", "Sandra Zgheib", "+961 70 567 890", "Verdun", "Ampere", 5, new DateTime(2024, 2, 14), "overdue", 85),
        new SubscriberData("c15", "Marwan Tabbara", "+961 76 678 901", "Gemmayzeh", "FixedKilowatt", 30, new DateTime(2023, 8, 1), "paid", 0),
    };

    public static readonly InvoiceData[] Invoices = new[]
    {
        new InvoiceData("i1", 1001, "Ahmad Khalil", "Hamra", 85, 85, 0, "paid", new DateTime(2025, 5, 1), new DateTime(2025, 6, 15), new DateTime(2025, 6, 14)),
        new InvoiceData("i2", 1002, "Rania Mansour", "Achrafieh", 150, 0, 150, "unpaid", new DateTime(2025, 5, 1), new DateTime(2025, 5, 28), new DateTime(2025, 5, 1)),
        new InvoiceData("i3", 1003, "Khalid Barakat", "Verdun", 85, 85, 0, "paid", new DateTime(2025, 5, 1), new DateTime(2025, 6, 20), new DateTime(2025, 6, 13)),
        new InvoiceData("i4", 1004, "Lara Haddad", "Gemmayzeh", 210, 0, 210, "overdue", new DateTime(2025, 4, 1), new DateTime(2025, 5, 10), new DateTime(2025, 4, 1)),
        new InvoiceData("i5", 1005, "Hassan Nassar", "Mar Elias", 85, 85, 0, "paid", new DateTime(2025, 5, 1), new DateTime(2025, 6, 5), new DateTime(2025, 6, 3)),
        new InvoiceData("i6", 1006, "Nadia Rizk", "Badaro", 150, 50, 100, "partiallyPaid", new DateTime(2025, 5, 1), new DateTime(2025, 5, 25), new DateTime(2025, 5, 1)),
        new InvoiceData("i7", 1007, "Fadi Gemayel", "Sodeco", 280, 280, 0, "paid", new DateTime(2025, 5, 1), new DateTime(2025, 6, 1), new DateTime(2025, 5, 28)),
        new InvoiceData("i8", 1008, "Carla Khoury", "Ras Beirut", 85, 85, 0, "paid", new DateTime(2025, 5, 1), new DateTime(2025, 6, 20), new DateTime(2025, 6, 18)),
        new InvoiceData("i9", 1009, "Rami Assaf", "Raouche", 150, 0, 150, "overdue", new DateTime(2025, 4, 1), new DateTime(2025, 5, 1), new DateTime(2025, 4, 1)),
        new InvoiceData("i10", 1010, "Maya Frem", "Tallet el Khayat", 210, 210, 0, "paid", new DateTime(2025, 5, 1), new DateTime(2025, 6, 25), new DateTime(2025, 6, 20)),
        new InvoiceData("i11", 1011, "Elie Saab", "Monot", 85, 0, 85, "unpaid", new DateTime(2025, 5, 1), new DateTime(2025, 5, 30), new DateTime(2025, 5, 1)),
        new InvoiceData("i12", 1012, "Sandra Zgheib", "Verdun", 85, 0, 85, "overdue", new DateTime(2025, 4, 1), new DateTime(2025, 5, 14), new DateTime(2025, 4, 1)),
    };

    public static readonly ExpenseData[] Expenses = new[]
    {
        new ExpenseData("e1", "Diesel refill - 500L", "Fuel", 620, "Hamra", new DateTime(2025, 6, 14), null),
        new ExpenseData("e2", "Generator B oil change", "Maintenance", 340, "Achrafieh", new DateTime(2025, 6, 12), "Quarterly maintenance"),
        new ExpenseData("e3", "Diesel refill - 400L", "Fuel", 495, "Verdun", new DateTime(2025, 6, 10), null),
        new ExpenseData("e4", "Replacement cables", "Parts", 280, "Hamra", new DateTime(2025, 6, 8), "BX-001 cable replacement"),
        new ExpenseData("e5", "Diesel refill - 600L", "Fuel", 745, "Gemmayzeh", new DateTime(2025, 6, 5), null),
        new ExpenseData("e6", "Filter replacement", "Maintenance", 150, "Badaro", new DateTime(2025, 6, 3), "Air and oil filters"),
        new ExpenseData("e7", "Diesel refill - 450L", "Fuel", 560, "Sodeco", new DateTime(2025, 6, 1), null),
        new ExpenseData("e8", "Technician wages - June", "Employees", 1200, null, new DateTime(2025, 5, 28), "Monthly salary"),
        new ExpenseData("e9", "Office supplies", "Other", 85, null, new DateTime(2025, 5, 20), "Paper, ink, stationery"),
        new ExpenseData("e10", "Insurance premium", "Other", 450, null, new DateTime(2025, 5, 15), "Annual equipment insurance"),
    };

    public static readonly NavigationItem[] NavItems = new[]
    {
        new NavigationItem("", "Dashboard"),
        new NavigationItem("areas", "Areas"),
        new NavigationItem("ampere-schedules", "Ampere Schedules"),
        new NavigationItem("boxes", "Boxes"),
        new NavigationItem("subscribers", "Subscribers"),
        new NavigationItem("invoices", "Invoices"),
        new NavigationItem("expenses", "Expenses"),
        new NavigationItem("settings", "Settings"),
    };

    public static readonly NavigationItem[] MobileNavItems = new[]
    {
        new NavigationItem("", "Dashboard"),
        new NavigationItem("subscribers", "Subscribers"),
        new NavigationItem("invoices", "Invoices"),
        new NavigationItem("settings", "Settings"),
    };

    public static string FormatCurrency(double value) =>
        value == 0 ? "$0" : $"${value:N0}";

    public static string FormatDate(DateTime date) =>
        date.ToString("MMM dd, yyyy");

    public static string FormatCompactCurrency(double value)
    {
        if (value >= 1_000_000) return $"${value / 1_000_000:0.#}M";
        if (value >= 1_000) return $"${value / 1_000:0.#}K";
        return $"${value:N0}";
    }

    public static bool IsOverdue(DateTime dueDate)
    {
        var today = DateTime.Today;
        return dueDate.Date < today;
    }
}
