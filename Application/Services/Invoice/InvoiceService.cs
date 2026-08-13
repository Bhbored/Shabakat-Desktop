using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Invoices;
using Shabakat.Application.DTOs.Payment;
using Shabakat.Application.Helper;
using Shabakat.Application.Services.Pricing;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Invoices;

public sealed class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAppPreferencesRepository _preferencesRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMeterReadingRepository _meterReadingRepository;
    private readonly IInvoiceSkipRepository _invoiceSkipRepository;
    private readonly IPricingService _pricingService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        ICustomerRepository customerRepository,
        IAppPreferencesRepository preferencesRepository,
        IPaymentRepository paymentRepository,
        IMeterReadingRepository meterReadingRepository,
        IInvoiceSkipRepository invoiceSkipRepository,
        IPricingService pricingService,
        IAuditLogService auditLogService,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _customerRepository = customerRepository;
        _preferencesRepository = preferencesRepository;
        _paymentRepository = paymentRepository;
        _meterReadingRepository = meterReadingRepository;
        _invoiceSkipRepository = invoiceSkipRepository;
        _pricingService = pricingService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<PagedResponse<InvoiceSummaryResponse>> GetAllAsync(InvoiceFilterRequest filter)
    {
        var (items, totalCount) = await _invoiceRepository.GetAllPagedAsync(filter);

        return PagedResponse<InvoiceSummaryResponse>.Create(
            data: items.Select(MapToSummary),
            totalCount: totalCount,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize);
    }

    public async Task<IEnumerable<InvoiceResponse>> GetAllUnpagedAsync()
    {
        var items = await _invoiceRepository.GetAllWithCustomerAsync();
        return items.Select(MapToResponse);
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetByIdWithPaymentsAsync(id)
            ?? throw new DomainException("Invoice not found.");

        return MapToResponse(invoice);
    }

    public async Task CreateAsync(CreateInvoiceRequest request)
    {
        var customer = await _customerRepository.GetByIdWithInvoicesAsync(request.CustomerId)
            ?? throw new DomainException("Customer not found.");

        if (customer.Plan == PlanType.FixedKilowatt)
        {
            await CreateFixedKilowattInvoiceAsync(customer, request);
            return;
        }

        var preferences = await _preferencesRepository.GetAsync()
            ?? throw new DomainException("App preferences have not been configured.");

        var today = DateOnly.FromDateTime(DateTime.Now);

        int? ampereBilledDays = null;
        if (customer.Plan == PlanType.Ampere)
        {
            ValidateAmpereBilledDays(request.BilledDays, preferences.AmpereProrateByDaysEnabled, today);
            if (preferences.AmpereProrateByDaysEnabled)
                ampereBilledDays = request.BilledDays;
        }

        var prepared = await PrepareStandardInvoiceAsync(
            customer, preferences, today, ampereBilledDays);

        if (!prepared.Success)
            throw await HandlePrepareFailureAsync(
                customer, prepared, preferences.Language, recordSkipOnMeterReadingOnly: true);

        var invoiceNumber = await _invoiceRepository.GetNextInvoiceNumberAsync();
        var invoice = await PersistStandardInvoiceAsync(customer, prepared, invoiceNumber);
        await _invoiceRepository.SaveChangesAsync();

        await _auditLogService.LogSuccessAsync(
            AuditLogEntries.InvoiceCreated(invoice, customer.Name, customer.Plan.ToString()));

        _logger.LogInformation(
            "Created invoice #{InvoiceNumber} for customer {CustomerId} ({CustomerName})",
            invoice.InvoiceNumber,
            customer.Id,
            customer.Name);
    }

    public async Task<BulkCreateInvoiceResponse> BulkCreateAsync(PlanType? planType = null)
    {
        if (planType is PlanType.FixedKilowatt)
        {
            throw new DomainException(
                "Bulk invoice creation supports only Ampere and Kilowatt. Omit planType to process both.");
        }

        var preferences = await _preferencesRepository.GetAsync()
            ?? throw new DomainException("App preferences have not been configured.");

        var today = DateOnly.FromDateTime(DateTime.Now);
        var (ampereStart, ampereEnd) = BillingPeriodHelper.GetBillingMonthBounds(
            PlanType.Ampere, today);
        var (kilowattStart, kilowattEnd) = BillingPeriodHelper.GetBillingMonthBounds(
            PlanType.Kilowatt, today);

        var customers = (await _customerRepository
            .GetActiveWithoutInvoiceAsync(ampereStart, ampereEnd, kilowattStart, kilowattEnd))
            .ToList();

        if (planType is PlanType.Ampere or PlanType.Kilowatt)
            customers = customers.Where(c => c.Plan == planType.Value).ToList();

        var nextInvoiceNumber = await _invoiceRepository.GetNextInvoiceNumberAsync();
        var created = 0;
        var skipped = 0;

        foreach (var customer in customers)
        {
            var result = await TryCreateBulkInvoiceAsync(
                customer, preferences, today, nextInvoiceNumber);
            if (result.Created)
            {
                created++;
                nextInvoiceNumber = result.NextInvoiceNumber;
            }
            else if (result.Skipped)
            {
                skipped++;
            }
        }

        if (today.Day >= 25 && planType is not PlanType.Kilowatt)
        {
            var nextMonthRef = new DateOnly(today.Year, today.Month, 1).AddMonths(1);
            var (nextStart, nextEnd) = BillingPeriodHelper.GetBillingMonthBounds(
                PlanType.Ampere, nextMonthRef);

            var ampereNextMonth = (await _customerRepository
                .GetActiveAmpereReadyForNextMonthAsync(
                    ampereStart, ampereEnd, nextStart, nextEnd))
                .ToList();

            foreach (var customer in ampereNextMonth)
            {
                var result = await TryCreateBulkInvoiceAsync(
                    customer, preferences, nextMonthRef, nextInvoiceNumber);
                if (result.Created)
                {
                    created++;
                    nextInvoiceNumber = result.NextInvoiceNumber;
                }
                else if (result.Skipped)
                {
                    skipped++;
                }
            }
        }

        if (created == 0 && skipped == 0)
        {
            return new BulkCreateInvoiceResponse(
                Created: 0,
                Skipped: 0,
                Message: InvoiceSkipMessages.AllCustomersAlreadyInvoiced(preferences.Language));
        }

        await _invoiceRepository.SaveChangesAsync();

        await _auditLogService.LogSuccessAsync(
            AuditLogEntries.InvoiceBulkCreated(created, skipped));

        _logger.LogInformation(
            "Bulk invoice create finished: {Created} created, {Skipped} skipped (plan filter: {PlanType})",
            created,
            skipped,
            planType?.ToString() ?? "all");

        return new BulkCreateInvoiceResponse(
            Created: created,
            Skipped: skipped,
            Message: InvoiceSkipMessages.CustomersSkippedSummary(preferences.Language, skipped));
    }

    private async Task<(bool Created, bool Skipped, int NextInvoiceNumber)> TryCreateBulkInvoiceAsync(
        Customer customer,
        AppPreferences preferences,
        DateOnly billingReferenceDate,
        int nextInvoiceNumber)
    {
        var prepared = await PrepareStandardInvoiceAsync(
            customer, preferences, billingReferenceDate);

        if (!prepared.Success)
        {
            await RecordInvoiceSkipAsync(
                customer, prepared.ConsumptionStart, prepared.ConsumptionEnd, prepared.Message!);
            return (false, true, nextInvoiceNumber);
        }

        try
        {
            await PersistStandardInvoiceAsync(customer, prepared, nextInvoiceNumber);
            return (true, false, nextInvoiceNumber + 1);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Bulk invoice skipped for customer {CustomerId} ({CustomerName})",
                customer.Id,
                customer.Name);
            await RecordInvoiceSkipAsync(
                customer, prepared.ConsumptionStart, prepared.ConsumptionEnd, ex.Message);
            return (false, true, nextInvoiceNumber);
        }
    }

    public async Task<IEnumerable<InvoiceSkippedResponse>> GetSkippedAsync()
    {
        var preferences = await _preferencesRepository.GetAsync();
        var language = preferences?.Language ?? "en";

        var skips = await _invoiceSkipRepository.GetAllAsync();

        return skips.Select(s => new InvoiceSkippedResponse(
            CustomerId: s.CustomerId,
            CustomerName: s.CustomerName,
            Reason: InvoiceSkipMessages.LocalizeStoredReason(
                s.Reason,
                language,
                s.CustomerName,
                s.BillingPeriodStart,
                s.BillingPeriodEnd),
            SkippedAt: s.CreatedAt));
    }

    public async Task PayAsync(Guid invoiceId, AddPaymentRequest request)
    {
        Invoice? paidInvoice = null;

        await _invoiceRepository.ExecuteInTransactionAsync(async () =>
        {
            var invoice = await _invoiceRepository.GetByIdForUpdateAsync(invoiceId)
                ?? throw new DomainException("Invoice not found.");

            if (invoice.InvoiceStatus == InvoiceStatus.Paid)
                throw new DomainException("This invoice is already fully paid.");

            var newTotalPaid = invoice.PaidAmount + request.Amount;
            if (newTotalPaid > invoice.TotalAmount)
            {
                throw new DomainException(
                    $"Payment of {request.Amount:F4} would exceed the invoice total. " +
                    $"Outstanding balance is {invoice.AmountDue:F4}.");
            }

            var payment = new Payment
            {
                CustomerId = invoice.CustomerId,
                InvoiceId = invoice.Id,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = DateTime.Now,
                Notes = request.Notes?.Trim()
            };

            await _paymentRepository.AddAsync(payment);

            invoice.PaidAmount += request.Amount;
            invoice.InvoiceStatus = ComputeStatus(invoice.PaidAmount, invoice.TotalAmount);
            _invoiceRepository.Update(invoice);

            await _invoiceRepository.SaveChangesAsync();
            paidInvoice = invoice;
        });

        if (paidInvoice is not null)
        {
            await _auditLogService.LogSuccessAsync(
                AuditLogEntries.InvoicePaymentRecorded(
                    paidInvoice, request.Amount, request.PaymentMethod));

            _logger.LogInformation(
                "Recorded payment of {Amount} on invoice #{InvoiceNumber} ({InvoiceId})",
                request.Amount,
                paidInvoice.InvoiceNumber,
                paidInvoice.Id);
        }
    }

    public async Task<InvoiceResponse> UpdateAsync(Guid id, UpdateInvoiceRequest request)
    {
        var invoice = await _invoiceRepository.GetByIdWithPaymentsAsync(id)
            ?? throw new DomainException("Invoice not found.");

        var newConsumptionStart = request.ConsumptionStart ?? invoice.IssueDate;
        var newConsumptionEnd = request.ConsumptionEnd ?? invoice.DueDate;

        if (newConsumptionEnd < newConsumptionStart)
        {
            throw new DomainException(
                "Consumption end must be on or after the consumption start date.");
        }

        if (request.ConsumptionStart is not null) invoice.IssueDate = request.ConsumptionStart.Value;
        if (request.ConsumptionEnd is not null) invoice.DueDate = request.ConsumptionEnd.Value;

        _invoiceRepository.Update(invoice);
        await _invoiceRepository.SaveChangesAsync();

        _logger.LogInformation("Updated invoice {InvoiceId} (#{InvoiceNumber})", invoice.Id, invoice.InvoiceNumber);
        return MapToResponse(invoice);
    }

    public async Task DeleteAsync(Guid id)
    {
        var invoice = await _invoiceRepository.GetByIdWithPaymentsAsync(id)
            ?? throw new DomainException("Invoice not found.");

        if (invoice.InvoiceStatus != InvoiceStatus.Unpaid)
        {
            throw new DomainException(
                "Only unpaid invoices can be deleted.");
        }

        var number = invoice.InvoiceNumber;
        _invoiceRepository.Delete(invoice);
        await _invoiceRepository.SaveChangesAsync();
        _logger.LogInformation("Deleted invoice {InvoiceId} (#{InvoiceNumber})", id, number);
    }

    public async Task<IEnumerable<PaymentResponse>> GetPaymentsAsync(Guid invoiceId)
    {
        _ = await _invoiceRepository.GetByIdAsync(invoiceId)
            ?? throw new DomainException("Invoice not found.");

        var payments = await _paymentRepository.GetByInvoiceIdAsync(invoiceId);
        return payments.Select(MapPaymentToResponse);
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllPaymentsUnpagedAsync()
    {
        var payments = await _paymentRepository.GetAllWithCustomerAsync();
        return payments.Select(MapPaymentToResponse);
    }

    public async Task<FixedKilowattCalculateResponse> CalculateFixedKilowattAsync(
        FixedKilowattCalculateRequest request)
    {
        var preferences = await _preferencesRepository.GetAsync()
            ?? throw new DomainException("App preferences have not been configured.");

        var rates = _pricingService.GetRates(
            request.CustomerType, PlanType.FixedKilowatt, preferences);

        if (rates.UnitPrice <= 0)
            throw new DomainException("Kilowatt unit price is not configured for this customer type.");

        var effectivePlanValue = request.PlanValue is > 0 ? request.PlanValue.Value : 0m;

        var (paymentAmount, kilowattCredits) = ResolveFixedKilowattAmounts(
            request.PaymentAmount,
            request.KilowattAmount,
            effectivePlanValue,
            rates.UnitPrice,
            rates.FixedCharge,
            rates.Tva);

        return new FixedKilowattCalculateResponse(
            PaymentAmount: paymentAmount,
            KilowattAmount: kilowattCredits,
            UnitPrice: rates.UnitPrice,
            FixedCharge: rates.FixedCharge,
            TVA: rates.Tva,
            PlanValue: effectivePlanValue,
            CustomerType: request.CustomerType.ToString());
    }

    private async Task CreateFixedKilowattInvoiceAsync(
        Customer customer,
        CreateInvoiceRequest request)
    {
        if (request.PaymentMethod is null)
            throw new DomainException("Payment method is required for FixedKilowatt customers.");

        var preferences = await _preferencesRepository.GetAsync()
            ?? throw new DomainException("App preferences have not been configured.");

        var rates = _pricingService.GetRates(customer, preferences);
        if (rates.UnitPrice <= 0)
            throw new DomainException("Kilowatt unit price is not configured for this customer.");

        var fixedCharge = rates.FixedCharge;
        var tva = rates.Tva;
        var unitPrice = rates.UnitPrice;

        var today = DateOnly.FromDateTime(DateTime.Now);
        Invoice? createdInvoice = null;
        decimal paymentAmount = 0m;
        decimal kilowattCredits = 0m;

        await _invoiceRepository.ExecuteInTransactionAsync(async () =>
        {
            var monthStart = new DateOnly(today.Year, today.Month, 1);
            var monthEndInclusive = monthStart.AddMonths(1).AddDays(-1);

            var planAlreadyChargedThisMonth = await _invoiceRepository.ExistsForCustomerInPeriodAsync(
                customer.Id, monthStart, monthEndInclusive);
            var effectivePlanValue = planAlreadyChargedThisMonth ? 0m : customer.PlanValue;

            (paymentAmount, kilowattCredits) = ResolveFixedKilowattAmounts(
                request.PaymentAmount,
                request.KilowattAmount,
                effectivePlanValue,
                unitPrice,
                fixedCharge,
                tva);

            var meterPeriodEnd = monthStart.AddMonths(1);
            var existingThisMonth = await _meterReadingRepository.GetForPeriodAsync(
                customer.Id, monthStart, meterPeriodEnd);

            if (existingThisMonth is not null)
            {
                existingThisMonth.ReadingValue += kilowattCredits;
                _meterReadingRepository.Update(existingThisMonth);
            }
            else
            {
                var previousReading = await _meterReadingRepository.GetLatestForCustomerAsync(customer.Id);
                var newReadingValue = previousReading is null
                    ? kilowattCredits
                    : previousReading.ReadingValue + kilowattCredits;

                await _meterReadingRepository.AddAsync(new MeterReading
                {
                    CustomerId = customer.Id,
                    ReadingValue = newReadingValue,
                    ReadingDate = today,
                    IsInitial = false
                });
            }

            var invoiceNumber = await _invoiceRepository.GetNextInvoiceNumberAsync();
            var invoice = new Invoice
            {
                CustomerId = customer.Id,
                InvoiceNumber = invoiceNumber,
                IssueDate = today,
                DueDate = today,
                FixedCharge = fixedCharge,
                TVA = tva,
                TotalAmount = paymentAmount,
                PaidAmount = paymentAmount,
                BilledConsumption = kilowattCredits,
                InvoiceStatus = InvoiceStatus.Paid
            };

            await _invoiceRepository.AddAsync(invoice);

            await _paymentRepository.AddAsync(new Payment
            {
                CustomerId = customer.Id,
                InvoiceId = invoice.Id,
                Amount = paymentAmount,
                PaymentMethod = request.PaymentMethod.Value,
                PaymentDate = DateTime.Now,
                Notes = request.Notes?.Trim()
            });

            await _invoiceRepository.SaveChangesAsync();
            createdInvoice = invoice;
        });

        if (createdInvoice is not null)
        {
            await _auditLogService.LogSuccessAsync(
                AuditLogEntries.InvoiceFixedKilowattCharge(
                    createdInvoice,
                    customer.Name,
                    paymentAmount,
                    kilowattCredits));

            _logger.LogInformation(
                "Created fixed-kW invoice #{InvoiceNumber} for {CustomerName} (amount {Amount}, kWh {Credits})",
                createdInvoice.InvoiceNumber,
                customer.Name,
                paymentAmount,
                kilowattCredits);
        }
    }

    private static (decimal PaymentAmount, decimal KilowattCredits) ResolveFixedKilowattAmounts(
        decimal? paymentAmount,
        decimal? kilowattAmount,
        decimal planValue,
        decimal unitPrice,
        decimal fixedCharge,
        decimal tva)
    {
        var hasPayment = paymentAmount is > 0;
        var hasKilowatt = kilowattAmount is > 0;

        if (hasPayment && hasKilowatt)
        {
            throw new DomainException(
                "Provide either paymentAmount or kilowattAmount, not both.");
        }

        if (!hasPayment && !hasKilowatt)
        {
            throw new DomainException(
                "Either paymentAmount or kilowattAmount is required.");
        }

        if (hasPayment)
        {
            var payment = paymentAmount!.Value;
            var kilowattCredits = InvoiceCalculationHelper.CalculateFixedKilowattConsumption(
                payment, planValue, unitPrice, fixedCharge, tva);

            if (kilowattCredits <= 0)
            {
                throw new DomainException(
                    "Payment amount is too low to cover the plan value, fixed charge, and energy at the configured rate.");
            }

            return (payment, kilowattCredits);
        }

        var credits = kilowattAmount!.Value;
        var total = InvoiceCalculationHelper.CalculateKilowattTotal(
            credits, planValue, unitPrice, fixedCharge, tva);

        if (total <= 0)
        {
            throw new DomainException(
                "Kilowatt amount is too low to produce a valid charge at the configured rate.");
        }

        return (total, credits);
    }

    private async Task<decimal?> TryGetKilowattConsumptionAsync(
        Guid customerId,
        DateOnly consumptionStart,
        DateOnly consumptionEnd)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var periodEnd = BillingPeriodHelper.ResolveKilowattMeterReadingPeriodEnd(
            consumptionEnd, today);

        var periodReading = await _meterReadingRepository.GetForPeriodAsync(
            customerId, consumptionStart, periodEnd);

        if (periodReading is null)
            return null;

        var previousReading = await _meterReadingRepository.GetLatestBeforeAsync(
            customerId, periodReading.ReadingDate, periodReading.Id);

        return InvoiceCalculationHelper.CalculateConsumption(
            periodReading.ReadingValue, previousReading);
    }

    private static InvoiceStatus ComputeStatus(decimal paid, decimal total) =>
        paid <= 0 ? InvoiceStatus.Unpaid :
        paid >= total ? InvoiceStatus.Paid :
                        InvoiceStatus.PartiallyPaid;

    private enum StandardInvoicePrepareError
    {
        UnsupportedPlan,
        DuplicatePeriod,
        MissingMeterReading,
        SchedulePricingRequired
    }

    private sealed record StandardInvoicePrepareResult(
        bool Success,
        DateOnly ConsumptionStart,
        DateOnly ConsumptionEnd,
        decimal TotalAmount,
        decimal FixedCharge,
        decimal Tva,
        StandardInvoicePrepareError? Error,
        string? Message)
    {
        public static StandardInvoicePrepareResult Succeeded(
            DateOnly consumptionStart,
            DateOnly consumptionEnd,
            decimal totalAmount,
            decimal fixedCharge,
            decimal tva) =>
            new(true, consumptionStart, consumptionEnd,
                totalAmount, fixedCharge, tva, null, null);

        public static StandardInvoicePrepareResult Failed(
            StandardInvoicePrepareError error,
            string message,
            DateOnly consumptionStart,
            DateOnly consumptionEnd) =>
            new(false, consumptionStart, consumptionEnd,
                0, 0, 0, error, message);
    }

    private async Task<StandardInvoicePrepareResult> PrepareStandardInvoiceAsync(
        Customer customer,
        AppPreferences preferences,
        DateOnly today,
        int? ampereBilledDays = null)
    {
        var (billingMonthStart, billingMonthEnd) = BillingPeriodHelper.GetBillingMonthBounds(
            customer.Plan, today);

        var (consumptionStart, consumptionEnd) = BillingPeriodHelper.ResolveConsumptionPeriod(
            customer.Plan, today);

        if (customer.Plan is not PlanType.Ampere and not PlanType.Kilowatt)
        {
            return StandardInvoicePrepareResult.Failed(
                StandardInvoicePrepareError.UnsupportedPlan,
                InvoiceSkipMessages.UnsupportedPlan(preferences.Language, customer.Plan.ToString()),
                consumptionStart,
                consumptionEnd);
        }

        if (await _invoiceRepository.ExistsForCustomerInPeriodAsync(
                customer.Id, billingMonthStart, billingMonthEnd))
        {
            return StandardInvoicePrepareResult.Failed(
                StandardInvoicePrepareError.DuplicatePeriod,
                InvoiceSkipMessages.DuplicatePeriod(
                    preferences.Language,
                    customer.Name,
                    billingMonthStart,
                    billingMonthEnd),
                consumptionStart,
                consumptionEnd);
        }

        PricingRates rates;
        try
        {
            rates = _pricingService.GetRates(customer, preferences);
        }
        catch (DomainException ex)
        {
            return StandardInvoicePrepareResult.Failed(
                StandardInvoicePrepareError.SchedulePricingRequired,
                ex.Message,
                consumptionStart,
                consumptionEnd);
        }

        decimal totalAmount;

        if (customer.Plan == PlanType.Ampere)
        {
            int? billedDays = null;
            int? daysInMonth = null;

            if (preferences.AmpereProrateByDaysEnabled && ampereBilledDays.HasValue)
            {
                billedDays = ampereBilledDays;
                daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
            }

            totalAmount = InvoiceCalculationHelper.CalculateAmpereTotal(
                customer.PlanValue,
                rates.UnitPrice,
                rates.FixedCharge,
                rates.Tva,
                billedDays,
                daysInMonth);
        }
        else
        {
            var consumption = await TryGetKilowattConsumptionAsync(
                customer.Id, consumptionStart, consumptionEnd);

            if (consumption is null)
            {
                return StandardInvoicePrepareResult.Failed(
                    StandardInvoicePrepareError.MissingMeterReading,
                    InvoiceSkipMessages.MissingMeterReading(
                        preferences.Language,
                        customer.Name,
                        consumptionStart,
                        consumptionEnd),
                    consumptionStart,
                    consumptionEnd);
            }

            totalAmount = InvoiceCalculationHelper.CalculateKilowattTotal(
                consumption.Value, customer.PlanValue, rates.UnitPrice, rates.FixedCharge, rates.Tva);
        }

        return StandardInvoicePrepareResult.Succeeded(
            consumptionStart, consumptionEnd,
            totalAmount, rates.FixedCharge, rates.Tva);
    }

    private async Task<Exception> HandlePrepareFailureAsync(
        Customer customer,
        StandardInvoicePrepareResult prepared,
        string language,
        bool recordSkipOnMeterReadingOnly)
    {
        if (recordSkipOnMeterReadingOnly &&
            prepared.Error == StandardInvoicePrepareError.MissingMeterReading)
        {
            await RecordInvoiceSkipAsync(
                customer, prepared.ConsumptionStart, prepared.ConsumptionEnd, prepared.Message!);
            await _invoiceSkipRepository.SaveChangesAsync();
        }

        return prepared.Error switch
        {
            StandardInvoicePrepareError.DuplicatePeriod =>
                new DomainException(prepared.Message!),
            StandardInvoicePrepareError.MissingMeterReading =>
                new DomainException(InvoiceSkipMessages.CustomersSkippedSummary(language, 1)),
            StandardInvoicePrepareError.SchedulePricingRequired =>
                new DomainException(prepared.Message!),
            _ => new DomainException(prepared.Message!)
        };
    }

    private static void ValidateAmpereBilledDays(
        int? billedDays,
        bool prorateEnabled,
        DateOnly today)
    {
        var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

        if (prorateEnabled)
        {
            if (!billedDays.HasValue)
            {
                throw new DomainException(
                    "BilledDays is required when ampere prorate-by-days is enabled.");
            }

            if (billedDays.Value < 1 || billedDays.Value > daysInMonth)
            {
                throw new DomainException(
                    $"BilledDays must be between 1 and {daysInMonth} for the current month.");
            }
        }
        else if (billedDays.HasValue)
        {
            throw new DomainException(
                "BilledDays is only allowed when ampere prorate-by-days is enabled in preferences.");
        }
    }

    private async Task<Invoice> PersistStandardInvoiceAsync(
        Customer customer,
        StandardInvoicePrepareResult prepared,
        int invoiceNumber)
    {
        var invoice = new Invoice
        {
            CustomerId = customer.Id,
            InvoiceNumber = invoiceNumber,
            IssueDate = prepared.ConsumptionStart,
            DueDate = prepared.ConsumptionEnd,
            FixedCharge = prepared.FixedCharge,
            TVA = prepared.Tva,
            TotalAmount = prepared.TotalAmount,
            PaidAmount = 0,
            InvoiceStatus = InvoiceStatus.Unpaid
        };

        await _invoiceRepository.AddAsync(invoice);
        await _invoiceSkipRepository.DeleteForCustomerPeriodAsync(
            customer.Id, prepared.ConsumptionStart, prepared.ConsumptionEnd);

        return invoice;
    }

    private async Task RecordInvoiceSkipAsync(
        Customer customer,
        DateOnly billingPeriodStart,
        DateOnly billingPeriodEnd,
        string reason)
    {
        await _invoiceSkipRepository.UpsertAsync(new InvoiceSkip
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            BillingPeriodStart = billingPeriodStart,
            BillingPeriodEnd = billingPeriodEnd,
            Reason = reason
        });
    }

    private static InvoiceSummaryResponse MapToSummary(Invoice i) =>
        new(
            Id: i.Id,
            InvoiceNumber: i.InvoiceNumber,
            CustomerName: i.Customer?.Name ?? string.Empty,
            InvoiceStatus: i.InvoiceStatus.ToString(),
            ConsumptionStart: i.IssueDate,
            ConsumptionEnd: i.DueDate,
            TotalAmount: i.TotalAmount,
            PaidAmount: i.PaidAmount,
            AmountDue: i.AmountDue,
            BilledConsumption: i.BilledConsumption,
            CreatedAt: i.CreatedAt,
            CanBeDeleted: i.InvoiceStatus == InvoiceStatus.Unpaid);

    private static InvoiceResponse MapToResponse(Invoice i) =>
        new(
            Id: i.Id,
            InvoiceNumber: i.InvoiceNumber,
            CustomerName: i.Customer?.Name ?? string.Empty,
            CustomerPhone: i.Customer?.Phone,
            CustomerId: i.CustomerId,
            InvoiceStatus: i.InvoiceStatus.ToString(),
            ConsumptionStart: i.IssueDate,
            ConsumptionEnd: i.DueDate,
            FixedCharge: i.FixedCharge,
            TVA: i.TVA,
            TotalAmount: i.TotalAmount,
            PaidAmount: i.PaidAmount,
            AmountDue: i.AmountDue,
            BilledConsumption: i.BilledConsumption,
            CreatedAt: i.CreatedAt,
            UpdatedAt: i.UpdatedAt,
            Payments: i.Payments.Select(MapPaymentToResponse));

    private static PaymentResponse MapPaymentToResponse(Payment p) =>
        new(
            Id: p.Id,
            InvoiceId: p.InvoiceId,
            CustomerName: p.Customer?.Name ?? string.Empty,
            CustomerId: p.CustomerId,
            Amount: p.Amount,
            PaymentMethod: p.PaymentMethod.ToString(),
            PaymentDate: p.PaymentDate,
            Notes: p.Notes,
            CreatedAt: p.CreatedAt);
}
