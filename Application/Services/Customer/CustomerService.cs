using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Customers;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Customers;

public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IDistributionBoxRepository _distributionBoxRepository;
    private readonly IAmpereScheduleRepository _ampereScheduleRepository;
    private readonly IAppPreferencesRepository _preferencesRepository;
    private readonly IMeterReadingRepository _meterReadingRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepository,
        IDistributionBoxRepository distributionBoxRepository,
        IAmpereScheduleRepository ampereScheduleRepository,
        IAppPreferencesRepository preferencesRepository,
        IMeterReadingRepository meterReadingRepository,
        IAuditLogService auditLogService,
        ILogger<CustomerService> logger)
    {
        _customerRepository = customerRepository;
        _distributionBoxRepository = distributionBoxRepository;
        _ampereScheduleRepository = ampereScheduleRepository;
        _preferencesRepository = preferencesRepository;
        _meterReadingRepository = meterReadingRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<PagedResponse<CustomerSummaryResponse>> GetAllAsync(CustomerFilterRequest filter)
    {
        var (items, totalCount) = await _customerRepository
            .GetAllWithCurrentMonthInvoicesAsync(filter);

        return PagedResponse<CustomerSummaryResponse>.Create(
            data: items.Select(MapToSummary),
            totalCount: totalCount,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize);
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllUnpagedAsync()
    {
        var items = await _customerRepository.GetAllWithDetailsAsync();
        return items.Select(MapToResponse);
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdWithInvoicesAsync(id)
            ?? throw new DomainException("Customer not found.");

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
    {
        ValidatePricingOverride(request.PricingOverride);
        await EnsureBoxExistsAsync(request.BoxId);

        var preferences = await GetPreferencesAsync();
        var ampereScheduleId = CustomerBillingRules.NormalizeAmpereScheduleId(
            request.Plan, request.AmpereScheduleId);

        CustomerBillingRules.ValidateAmpereScheduleAssignment(
            request.Plan, ampereScheduleId, preferences);
        await EnsureAmpereScheduleExistsAsync(ampereScheduleId);

        var customer = new Customer
        {
            Name = request.Name.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            Building = request.Building?.Trim(),
            Floor = request.Floor?.Trim(),
            CableName = request.CableName?.Trim(),
            BoxId = request.BoxId,
            AmpereScheduleId = ampereScheduleId,
            CustomerType = request.CustomerType,
            SubscriptionDate = request.SubscriptionDate ?? DateOnly.FromDateTime(DateTime.Now),
            CustomerStatus = CustomerStatus.Active,
            Plan = request.Plan,
            AreaId = request.AreaId,
            PlanValue = request.PlanValue,
            CustomerRelation = request.CustomerRelation
        };

        if (request.PricingOverride is not null)
        {
            customer.SetPricingOverride(
                request.PricingOverride.Price!.Value,
                request.PricingOverride.FixedCharge!.Value,
                request.PricingOverride.TVA!.Value);
        }

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();
        await TryApplyInitialMeterReadingAsync(customer, request.InitialMeterReading);

        await _auditLogService.LogSuccessAsync(AuditLogEntries.CustomerCreated(customer));
        _logger.LogInformation(
            "Created customer {CustomerId} ({Name}, {Plan})",
            customer.Id,
            customer.Name,
            customer.Plan);

        var created = await _customerRepository.GetByIdWithInvoicesAsync(customer.Id)
            ?? throw new DomainException("Customer not found.");

        return MapToResponse(created);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        ValidatePricingOverride(request.PricingOverride);

        var customer = await _customerRepository.GetByIdWithInvoicesAsync(id)
            ?? throw new DomainException("Customer not found.");

        var preferences = await GetPreferencesAsync();
        var effectivePlan = request.Plan ?? customer.Plan;

        if (request.BoxId is not null)
        {
            await EnsureBoxExistsAsync(request.BoxId);
            customer.BoxId = request.BoxId;
        }

        if (request.Name is not null) customer.Name = request.Name.Trim();
        if (request.Phone is not null) customer.Phone = request.Phone.Trim();
        if (request.Address is not null) customer.Address = request.Address.Trim();
        if (request.Building is not null) customer.Building = request.Building.Trim();
        if (request.Floor is not null) customer.Floor = request.Floor.Trim();
        if (request.CableName is not null) customer.CableName = request.CableName.Trim();
        if (request.CustomerType is not null) customer.CustomerType = request.CustomerType.Value;
        if (request.Plan is not null) customer.Plan = request.Plan.Value;
        if (request.PlanValue is not null) customer.PlanValue = request.PlanValue.Value;
        if (request.CustomerStatus is not null) customer.CustomerStatus = request.CustomerStatus.Value;
        if (request.AreaId is not null) customer.AreaId = request.AreaId;

        if (request.AmpereScheduleId is not null || request.Plan is not null)
        {
            var ampereScheduleId = CustomerBillingRules.NormalizeAmpereScheduleId(
                effectivePlan,
                request.AmpereScheduleId ?? customer.AmpereScheduleId);

            CustomerBillingRules.ValidateAmpereScheduleAssignment(
                effectivePlan, ampereScheduleId, preferences);
            await EnsureAmpereScheduleExistsAsync(ampereScheduleId);

            customer.AmpereScheduleId = ampereScheduleId;
        }
        else if (effectivePlan != PlanType.Ampere)
        {
            customer.AmpereScheduleId = null;
        }

        customer.CustomerRelation = request.CustomerRelation;

        if (request.ClearPricingOverride)
            customer.ClearPricingOverride();
        else if (request.PricingOverride is not null)
        {
            customer.SetPricingOverride(
                request.PricingOverride.Price!.Value,
                request.PricingOverride.FixedCharge!.Value,
                request.PricingOverride.TVA!.Value);
        }

        _customerRepository.Update(customer);
        await _customerRepository.SaveChangesAsync();
        await TryApplyInitialMeterReadingAsync(customer, request.InitialMeterReading);

        var updated = await _customerRepository.GetByIdWithInvoicesAsync(customer.Id)
            ?? throw new DomainException("Customer not found.");

        _logger.LogInformation("Updated customer {CustomerId} ({Name})", customer.Id, customer.Name);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdWithInvoicesAsync(id)
            ?? throw new DomainException("Customer not found.");

        if (customer.Invoices.Any())
        {
            throw new DomainException(
                $"Cannot delete '{customer.Name}' because they have one or more invoices.");
        }

        var name = customer.Name;
        _customerRepository.Delete(customer);
        await _customerRepository.SaveChangesAsync();
        _logger.LogInformation("Deleted customer {CustomerId} ({Name})", id, name);
    }

    public async Task<SuspendCustomersResponse> SuspendAsync(SuspendCustomersRequest request)
    {
        var ids = request.CustomerIds.Distinct().ToList();
        var customers = await _customerRepository.GetByIdsAsync(ids);

        if (customers.Count != ids.Count)
        {
            var foundIds = customers.Select(c => c.Id).ToHashSet();
            var missingId = ids.First(id => !foundIds.Contains(id));
            throw new DomainException($"Customer not found: {missingId}.");
        }

        foreach (var customer in customers)
        {
            customer.CustomerStatus = CustomerStatus.Suspended;
            _customerRepository.Update(customer);
        }

        await _customerRepository.SaveChangesAsync();
        _logger.LogInformation("Suspended {Count} customer(s)", customers.Count);

        var message = customers.Count == 1
            ? "1 customer was suspended."
            : $"{customers.Count} customers were suspended.";

        return new SuspendCustomersResponse(
            Suspended: customers.Count,
            Message: message);
    }

    private static CustomerSummaryResponse MapToSummary(Customer c)
    {
        var amountDue = c.Invoices
            .Where(i => i.InvoiceStatus != InvoiceStatus.Paid)
            .Sum(i => i.AmountDue);

        return new CustomerSummaryResponse(
            Id: c.Id,
            Name: c.Name,
            Phone: c.Phone,
            Address: c.Address,
            Building: c.Building,
            Floor: c.Floor,
            CableName: c.CableName,
            BoxId: c.BoxId,
            BoxName: c.DistributionBox?.Name,
            AmpereScheduleId: c.AmpereScheduleId,
            AmpereScheduleName: c.AmpereSchedule?.Name,
            CustomerType: c.CustomerType.ToString(),
            Plan: c.Plan.ToString(),
            AreaName: c.Area?.Name,
            PlanValue: c.PlanValue,
            CustomerStatus: c.CustomerStatus.ToString(),
            SubscriptionDate: c.SubscriptionDate,
            CreatedAt: c.CreatedAt,
            HasPricingOverride: c.HasPricingOverride,
            CustomerRelation: c.CustomerRelation?.ToString(),
            AmountDue: amountDue);
    }

    private static CustomerResponse MapToResponse(Customer c)
    {
        return new CustomerResponse(
            Id: c.Id,
            Name: c.Name,
            Phone: c.Phone,
            AreaName: c.Area?.Name,
            Address: c.Address,
            Building: c.Building,
            Floor: c.Floor,
            CableName: c.CableName,
            BoxId: c.BoxId,
            BoxName: c.DistributionBox?.Name,
            AmpereScheduleId: c.AmpereScheduleId,
            AmpereScheduleName: c.AmpereSchedule?.Name,
            CustomerType: c.CustomerType.ToString(),
            Plan: c.Plan.ToString(),
            PlanValue: c.PlanValue,
            InitialMeterReading: c.MeterReadings
                .FirstOrDefault(m => m.IsInitial)?.ReadingValue,
            CustomerStatus: c.CustomerStatus.ToString(),
            SubscriptionDate: c.SubscriptionDate,
            CreatedAt: c.CreatedAt,
            CustomerRelation: c.CustomerRelation?.ToString(),
            HasPricingOverride: c.HasPricingOverride,
            PricingOverride: c.HasPricingOverride
                ? new CustomerPricingOverrideDto(
                    c.PriceOverride!.Value,
                    c.FixedChargeOverride!.Value,
                    c.TVAOverride!.Value)
                : null,
            TotalBilled: c.Invoices.Sum(i => i.TotalAmount),
            TotalPaid: c.Invoices.Sum(i => i.PaidAmount),
            TotalOutstanding: c.Invoices.Sum(i => i.AmountDue));
    }

    private static void ValidatePricingOverride(CustomerPricingOverrideDto? dto)
    {
        if (dto is null) return;

        if (!dto.Price.HasValue)
            throw new DomainException("Price is required when providing a pricing override.");
        if (dto.Price.Value < 0)
            throw new DomainException("Pricing override price cannot be negative.");

        if (!dto.FixedCharge.HasValue)
            throw new DomainException("FixedCharge is required when providing a pricing override.");
        if (dto.FixedCharge.Value < 0)
            throw new DomainException("Pricing override fixed charge cannot be negative.");

        if (!dto.TVA.HasValue)
            throw new DomainException("TVA is required when providing a pricing override.");
        if (dto.TVA.Value < 0 || dto.TVA.Value > 100)
            throw new DomainException("Pricing override TVA must be between 0 and 100.");
    }

    private async Task TryApplyInitialMeterReadingAsync(
        Customer customer,
        decimal? initialMeterReading)
    {
        if (initialMeterReading is not > 0)
            return;

        if (customer.Plan != PlanType.Kilowatt)
            return;

        if (await _meterReadingRepository.HasOfficialReadingAsync(customer.Id))
            return;

        var existingInitial = await _meterReadingRepository.GetInitialForCustomerAsync(customer.Id);
        if (existingInitial is not null)
        {
            existingInitial.ReadingValue = initialMeterReading.Value;
            _meterReadingRepository.Update(existingInitial);
        }
        else
        {
            await _meterReadingRepository.AddAsync(new MeterReading
            {
                CustomerId = customer.Id,
                ReadingValue = initialMeterReading.Value,
                ReadingDate = customer.SubscriptionDate,
                IsInitial = true
            });
        }

        await _meterReadingRepository.SaveChangesAsync();
    }

    private async Task EnsureBoxExistsAsync(Guid? boxId)
    {
        if (boxId is null) return;

        _ = await _distributionBoxRepository.GetByIdAsync(boxId.Value)
            ?? throw new DomainException("Distribution box not found.");
    }

    private async Task EnsureAmpereScheduleExistsAsync(Guid? ampereScheduleId)
    {
        if (ampereScheduleId is null) return;

        _ = await _ampereScheduleRepository.GetByIdAsync(ampereScheduleId.Value)
            ?? throw new DomainException("Ampere schedule not found.");
    }

    private async Task<AppPreferences> GetPreferencesAsync()
        => await _preferencesRepository.GetAsync()
            ?? throw new DomainException("App preferences have not been configured.");
}
