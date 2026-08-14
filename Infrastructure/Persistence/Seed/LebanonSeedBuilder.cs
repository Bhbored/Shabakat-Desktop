using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;

namespace Shabakat.Infrastructure.Persistence.Seed;

public static class LebanonSeedBuilder
{
    private const int CustomerCount = 150;
    private const int BoxesPerArea = 3;
    private const decimal FixedCharge = 5m;
    private const decimal Tva = 11m;

    private static readonly Lazy<SeedBundle> Cache = new(Build);

    public static Area[] Areas => Cache.Value.Areas;
    public static AmpereSchedule[] Schedules => Cache.Value.Schedules;
    public static DistributionBox[] Boxes => Cache.Value.Boxes;
    public static Customer[] Customers => Cache.Value.Customers;
    public static MeterReading[] Readings => Cache.Value.Readings;
    public static Invoice[] Invoices => Cache.Value.Invoices;
    public static Payment[] Payments => Cache.Value.Payments;
    public static Expenses[] Expenses => Cache.Value.Expenses;
    public static InvoiceSkip[] Skips => Cache.Value.Skips;

    private static SeedBundle Build()
    {
        var areas = BuildAreas();
        var schedules = BuildSchedules();
        var boxes = BuildBoxes(areas.Length);
        var customers = BuildCustomers(areas.Length, boxes.Length, schedules.Length);
        var readings = BuildReadings(customers);
        var (invoices, payments) = BuildInvoicesAndPayments(customers);
        var expenses = BuildExpenses();
        var skips = BuildSkips(customers);

        return new SeedBundle(areas, schedules, boxes, customers, readings, invoices, payments, expenses, skips);
    }

    private static Area[] BuildAreas()
    {
        var list = new Area[LebanonSeedCatalog.Areas.Length];
        for (var i = 0; i < list.Length; i++)
        {
            list[i] = new Area
            {
                Id = SeedIds.For(SeedIds.Area, i + 1),
                Name = LebanonSeedCatalog.Areas[i].Name,
                CreatedAt = SeedClock.Created,
                UpdatedAt = SeedClock.Created,
            };
        }

        return list;
    }

    private static AmpereSchedule[] BuildSchedules() =>
    [
        Schedule(1, "اشتراك ٨ ساعات", 8, 18m, 18m, 22m, 28m),
        Schedule(2, "اشتراك ١٢ ساعة", 12, 24m, 24m, 30m, 38m),
        Schedule(3, "اشتراك ١٦ ساعة", 16, 32m, 32m, 40m, 50m),
        Schedule(4, "اشتراك ٢٤ ساعة", 24, 45m, 45m, 55m, 70m),
    ];

    private static AmpereSchedule Schedule(
        int index, string name, int hours, decimal price, decimal residential, decimal commercial, decimal industrial)
        => new()
        {
            Id = SeedIds.For(SeedIds.Schedule, index),
            Name = name,
            HoursPerDay = hours,
            PricePerAmp = price,
            ResidentialPricePerAmp = residential,
            CommercialPricePerAmp = commercial,
            IndustrialPricePerAmp = industrial,
            CreatedAt = SeedClock.Created,
            UpdatedAt = SeedClock.Created,
        };

    private static DistributionBox[] BuildBoxes(int areaCount)
    {
        var list = new List<DistributionBox>(areaCount * BoxesPerArea);
        var index = 1;
        for (var a = 0; a < areaCount; a++)
        {
            var areaName = LebanonSeedCatalog.Areas[a].Name;
            var streets = LebanonSeedCatalog.Areas[a].Streets;
            for (var b = 0; b < BoxesPerArea; b++)
            {
                list.Add(new DistributionBox
                {
                    Id = SeedIds.For(SeedIds.Box, index),
                    AreaId = SeedIds.For(SeedIds.Area, a + 1),
                    Name = $"علبة {areaName} {ToArabicDigit(b + 1)}",
                    LocationNote = streets[b % streets.Length],
                    Notes = b == 0 ? "علبة رئيسية في المنطقة" : null,
                    CreatedAt = SeedClock.Created,
                    UpdatedAt = SeedClock.Created,
                });
                index++;
            }
        }

        return list.ToArray();
    }

    private static Customer[] BuildCustomers(int areaCount, int boxCount, int scheduleCount)
    {
        var customers = new Customer[CustomerCount];
        var businessIndex = 0;

        for (var i = 0; i < CustomerCount; i++)
        {
            var n = i + 1;
            var areaIndex = i % areaCount;
            var boxIndex = (i % boxCount) + 1;
            var slot = i % 10;

            var (type, plan, planValue, scheduleIndex, isBusiness) = slot switch
            {
                0 => (CustomerType.Commercial, PlanType.Kilowatt, 0m, (int?)null, true),
                1 => (CustomerType.Industrial, PlanType.FixedKilowatt, 150m + (i % 5) * 25m, null, true),
                2 => (CustomerType.Commercial, PlanType.Ampere, 15m + (i % 3) * 5m, 3, true),
                3 => (CustomerType.Industrial, PlanType.Ampere, 30m + (i % 4) * 10m, 4, true),
                4 => (CustomerType.Residential, PlanType.Ampere, 5m, 1, false),
                5 => (CustomerType.Residential, PlanType.Ampere, 10m, 2, false),
                6 => (CustomerType.Residential, PlanType.Ampere, 15m, 3, false),
                7 => (CustomerType.Commercial, PlanType.Kilowatt, 0m, null, true),
                8 => (CustomerType.Residential, PlanType.Ampere, 10m, 2, false),
                _ => (CustomerType.Residential, PlanType.Ampere, 5m + (i % 2) * 5m, (i % scheduleCount) + 1, false),
            };

            var status = (i % 17) switch
            {
                0 => CustomerStatus.Suspended,
                1 => CustomerStatus.Terminated,
                _ => CustomerStatus.Active,
            };

            CustomerRelation? relation = isBusiness
                ? (i % 3 == 0 ? CustomerRelation.Owner : null)
                : (CustomerRelation)(i % 3);

            var streets = LebanonSeedCatalog.Areas[areaIndex].Streets;
            var name = isBusiness
                ? LebanonSeedCatalog.BusinessNames[businessIndex++ % LebanonSeedCatalog.BusinessNames.Length]
                : BuildPersonName(i);

            customers[i] = new Customer
            {
                Id = SeedIds.For(SeedIds.Customer, n),
                Name = name,
                Phone = BuildPhone(i, areaIndex),
                Address = streets[i % streets.Length],
                Building = LebanonSeedCatalog.Buildings[i % LebanonSeedCatalog.Buildings.Length],
                Floor = LebanonSeedCatalog.Floors[i % LebanonSeedCatalog.Floors.Length],
                CableName = LebanonSeedCatalog.Cables[i % LebanonSeedCatalog.Cables.Length],
                AreaId = SeedIds.For(SeedIds.Area, areaIndex + 1),
                BoxId = SeedIds.For(SeedIds.Box, boxIndex),
                AmpereScheduleId = scheduleIndex is null ? null : SeedIds.For(SeedIds.Schedule, scheduleIndex.Value),
                CustomerType = type,
                CustomerRelation = relation,
                CustomerStatus = status,
                Plan = plan,
                PlanValue = planValue,
                SubscriptionDate = new DateOnly(2022 + (i % 3), (i % 12) + 1, Math.Min(28, 1 + (i % 27))),
                CreatedAt = SeedClock.Created,
                UpdatedAt = SeedClock.Created,
            };
        }

        return customers;
    }

    private static MeterReading[] BuildReadings(Customer[] customers)
    {
        var list = new List<MeterReading>();
        var readingIndex = 1;
        var months = new[]
        {
            new DateOnly(2025, 3, 1),
            new DateOnly(2025, 4, 28),
            new DateOnly(2025, 5, 28),
            new DateOnly(2025, 6, 28),
            new DateOnly(2025, 7, 28),
        };

        for (var i = 0; i < customers.Length; i++)
        {
            var customer = customers[i];
            if (customer.Plan is not (PlanType.Kilowatt or PlanType.FixedKilowatt))
                continue;

            var baseValue = 1000m + (i * 37m);
            list.Add(Reading(readingIndex++, customer.Id, baseValue, months[0], isInitial: true));

            var value = baseValue;
            for (var m = 1; m < months.Length; m++)
            {
                if (i % 7 == 0 && m == 2)
                    continue;

                value += 180m + ((i + m) % 9) * 35m;
                var date = new DateOnly(months[m].Year, months[m].Month, Math.Min(28, 20 + (i % 8)));
                list.Add(Reading(readingIndex++, customer.Id, value, date));
            }
        }

        return list.ToArray();
    }

    private static MeterReading Reading(int index, Guid customerId, decimal value, DateOnly date, bool isInitial = false)
        => new()
        {
            Id = SeedIds.For(SeedIds.Reading, index),
            CustomerId = customerId,
            ReadingValue = Math.Round(value, 4),
            ReadingDate = date,
            IsInitial = isInitial,
            CreatedAt = SeedClock.Created,
            UpdatedAt = SeedClock.Created,
        };

    private static (Invoice[] Invoices, Payment[] Payments) BuildInvoicesAndPayments(Customer[] customers)
    {
        var invoices = new List<Invoice>();
        var payments = new List<Payment>();
        var invoiceNumber = 1000;
        var invoiceIndex = 1;
        var paymentIndex = 1;

        var periods = new[]
        {
            (Issue: new DateOnly(2025, 3, 1), Due: new DateOnly(2025, 3, 31)),
            (Issue: new DateOnly(2025, 4, 1), Due: new DateOnly(2025, 4, 30)),
            (Issue: new DateOnly(2025, 5, 1), Due: new DateOnly(2025, 5, 31)),
            (Issue: new DateOnly(2025, 6, 1), Due: new DateOnly(2025, 6, 30)),
            (Issue: new DateOnly(2025, 7, 1), Due: new DateOnly(2025, 7, 31)),
        };

        for (var i = 0; i < customers.Length; i++)
        {
            var customer = customers[i];
            if (customer.CustomerStatus == CustomerStatus.Terminated)
                continue;

            var periodCount = 3 + (i % 3);
            for (var p = 0; p < periodCount; p++)
            {
                var period = periods[p % periods.Length];
                invoiceNumber++;
                var invId = SeedIds.For(SeedIds.Invoice, invoiceIndex++);

                var total = customer.Plan switch
                {
                    PlanType.Ampere => Math.Round((customer.PlanValue * AmpUnitPrice(customer) + FixedCharge) * (1 + Tva / 100m), 4),
                    PlanType.Kilowatt => Math.Round(350m + (i % 11) * 45m + (p * 30m), 4),
                    PlanType.FixedKilowatt => Math.Round(customer.PlanValue + 400m + (i % 8) * 55m, 4),
                    _ => 100m,
                };

                var consumption = customer.Plan is PlanType.Kilowatt or PlanType.FixedKilowatt
                    ? 150m + (i % 13) * 25m + p * 20m
                    : (decimal?)null;

                var pattern = (i + p) % 5;
                decimal paid;
                InvoiceStatus status;
                switch (pattern)
                {
                    case 0:
                    case 1:
                        paid = total;
                        status = InvoiceStatus.Paid;
                        break;
                    case 2:
                        paid = Math.Round(total * 0.4m, 4);
                        status = InvoiceStatus.PartiallyPaid;
                        break;
                    default:
                        paid = 0m;
                        status = InvoiceStatus.Unpaid;
                        break;
                }

                if (customer.CustomerStatus == CustomerStatus.Suspended && p == periodCount - 1)
                {
                    paid = 0m;
                    status = InvoiceStatus.Unpaid;
                }

                invoices.Add(new Invoice
                {
                    Id = invId,
                    CustomerId = customer.Id,
                    InvoiceNumber = invoiceNumber,
                    IssueDate = period.Issue.AddDays(i % 5),
                    DueDate = period.Due,
                    FixedCharge = FixedCharge,
                    TVA = Tva,
                    TotalAmount = total,
                    PaidAmount = paid,
                    BilledConsumption = consumption,
                    InvoiceStatus = status,
                    CreatedAt = SeedClock.Created,
                    UpdatedAt = SeedClock.Created,
                });

                if (paid <= 0)
                    continue;

                if (status == InvoiceStatus.Paid)
                {
                    payments.Add(Pay(paymentIndex++, customer.Id, invId, paid,
                        (i + p) % 2 == 0 ? PaymentMethod.Cash : PaymentMethod.Wish,
                        period.Issue.AddDays(8 + (i % 10)).ToDateTime(new TimeOnly(10 + (i % 8), 15)),
                        (i + p) % 4 == 0 ? "دفعة شهرية" : null));
                }
                else
                {
                    var first = Math.Round(paid * 0.6m, 4);
                    var second = paid - first;
                    payments.Add(Pay(paymentIndex++, customer.Id, invId, first,
                        PaymentMethod.Cash,
                        period.Issue.AddDays(6).ToDateTime(new TimeOnly(11, 0)),
                        "دفعة جزئية ١"));
                    if (second > 0)
                    {
                        payments.Add(Pay(paymentIndex++, customer.Id, invId, second,
                            PaymentMethod.Wish,
                            period.Issue.AddDays(18).ToDateTime(new TimeOnly(16, 30)),
                            "دفعة جزئية ٢"));
                    }
                }
            }
        }

        return (invoices.ToArray(), payments.ToArray());
    }

    private static Payment Pay(
        int index, Guid customerId, Guid invoiceId, decimal amount, PaymentMethod method, DateTime date, string? notes)
        => new()
        {
            Id = SeedIds.For(SeedIds.Payment, index),
            CustomerId = customerId,
            InvoiceId = invoiceId,
            Amount = amount,
            PaymentMethod = method,
            PaymentDate = date,
            Notes = notes,
            CreatedAt = SeedClock.Created,
            UpdatedAt = SeedClock.Created,
        };

    private static Expenses[] BuildExpenses()
    {
        var list = new Expenses[60];
        var types = new[] { ExpenseType.Fuel, ExpenseType.Maintenance, ExpenseType.Employees, ExpenseType.Other };
        for (var i = 0; i < list.Length; i++)
        {
            var month = 2 + (i / 10);
            var day = 1 + (i % 28);
            var type = types[i % types.Length];
            var amount = type switch
            {
                ExpenseType.Fuel => 280m + (i % 9) * 35m,
                ExpenseType.Maintenance => 400m + (i % 7) * 80m,
                ExpenseType.Employees => 1100m + (i % 5) * 50m,
                _ => 50m + (i % 11) * 20m,
            };

            list[i] = new Expenses
            {
                Id = SeedIds.For(SeedIds.Expense, i + 1),
                ExpenseType = type,
                Amount = amount,
                ExpenseDate = new DateOnly(2025, Math.Clamp(month, 1, 8), day),
                Label = LebanonSeedCatalog.ExpenseLabels[i % LebanonSeedCatalog.ExpenseLabels.Length],
                Notes = i % 3 == 0 ? "مصروف تشغيلي للشبكة" : null,
                CreatedAt = SeedClock.Created,
                UpdatedAt = SeedClock.Created,
            };
        }

        return list;
    }

    private static InvoiceSkip[] BuildSkips(Customer[] customers)
    {
        var terminatedOrSuspended = customers
            .Where(c => c.CustomerStatus is CustomerStatus.Terminated or CustomerStatus.Suspended)
            .Take(12)
            .ToArray();

        var list = new InvoiceSkip[terminatedOrSuspended.Length];
        for (var i = 0; i < list.Length; i++)
        {
            var c = terminatedOrSuspended[i];
            list[i] = new InvoiceSkip
            {
                Id = SeedIds.For(SeedIds.Skip, i + 1),
                CustomerId = c.Id,
                CustomerName = c.Name,
                BillingPeriodStart = new DateOnly(2025, 7, 1),
                BillingPeriodEnd = new DateOnly(2025, 7, 31),
                Reason = c.CustomerStatus == CustomerStatus.Terminated
                    ? "اشتراك منتهٍ — لا فاتورة بعد الإنهاء"
                    : "موقوف مؤقتاً — تم تخطي الفترة",
                CreatedAt = SeedClock.Created,
                UpdatedAt = SeedClock.Created,
            };
        }

        return list;
    }

    private static decimal AmpUnitPrice(Customer customer) => customer.CustomerType switch
    {
        CustomerType.Commercial => 30m,
        CustomerType.Industrial => 40m,
        _ => 24m,
    };

    private static string BuildPersonName(int i)
    {
        var female = i % 2 == 0;
        var first = female
            ? LebanonSeedCatalog.FemaleFirstNames[i % LebanonSeedCatalog.FemaleFirstNames.Length]
            : LebanonSeedCatalog.MaleFirstNames[i % LebanonSeedCatalog.MaleFirstNames.Length];
        var last = LebanonSeedCatalog.LastNames[(i * 3) % LebanonSeedCatalog.LastNames.Length];
        return $"{first} {last}";
    }

    private static string BuildPhone(int i, int areaIndex)
    {
        var prefixes = new[] { "03", "71", "70", "76", "81", "01", "06", "07" };
        var prefix = prefixes[areaIndex % prefixes.Length];
        var mid = 100 + (i % 900);
        var last = 100 + ((i * 7) % 900);
        return $"{prefix} {mid:000} {last:000}";
    }

    private static string ToArabicDigit(int n) => n switch
    {
        1 => "١",
        2 => "٢",
        3 => "٣",
        4 => "٤",
        5 => "٥",
        _ => n.ToString(),
    };

    private sealed record SeedBundle(
        Area[] Areas,
        AmpereSchedule[] Schedules,
        DistributionBox[] Boxes,
        Customer[] Customers,
        MeterReading[] Readings,
        Invoice[] Invoices,
        Payment[] Payments,
        Expenses[] Expenses,
        InvoiceSkip[] Skips);
}
