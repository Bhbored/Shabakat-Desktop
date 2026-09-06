using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Customers;
using Shabakat.Application.Helper;
using Shabakat.Application.Mappers;
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
            data: items.Select(c => c.ToSummary()),
            totalCount: totalCount,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize);
    }

    public async Task<IEnumerable<CustomerResponse>> GetAllUnpagedAsync()
    {
        var items = await _customerRepository.GetAllWithDetailsAsync();
        return items.Select(c => c.ToResponse());
    }

    public async Task<CustomerResponse> GetByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdWithInvoicesAsync(id)
            ?? throw new DomainException("Error.CustomerNotFound");

        return customer.ToResponse();
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
            ?? throw new DomainException("Error.CustomerNotFound");

        return created.ToResponse();
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        ValidatePricingOverride(request.PricingOverride);

        var customer = await _customerRepository.GetByIdWithInvoicesAsync(id)
            ?? throw new DomainException("Error.CustomerNotFound");

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
            ?? throw new DomainException("Error.CustomerNotFound");

        _logger.LogInformation("Updated customer {CustomerId} ({Name})", customer.Id, customer.Name);
        return updated.ToResponse();
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdWithInvoicesAsync(id)
            ?? throw new DomainException("Error.CustomerNotFound");

        if (customer.Invoices.Any())
        {
            throw DomainException.Format("Error.CannotDeleteCustomer", customer.Name);
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
            throw new DomainException("Error.CustomerNotFound");
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

    public async Task<TerminateCustomersResponse> TerminateAsync(TerminateCustomersRequest request)
    {
        var ids = request.CustomerIds.Distinct().ToList();
        var customers = await _customerRepository.GetByIdsAsync(ids);

        if (customers.Count != ids.Count)
        {
            var foundIds = customers.Select(c => c.Id).ToHashSet();
            var missingId = ids.First(id => !foundIds.Contains(id));
            throw new DomainException("Error.CustomerNotFound");
        }

        foreach (var customer in customers)
        {
            customer.CustomerStatus = CustomerStatus.Terminated;
            _customerRepository.Update(customer);
        }

        await _customerRepository.SaveChangesAsync();
        _logger.LogInformation("Terminated {Count} customer(s)", customers.Count);

        var message = customers.Count == 1
            ? "1 customer was terminated."
            : $"{customers.Count} customers were terminated.";

        return new TerminateCustomersResponse(
            Terminated: customers.Count,
            Message: message);
    }

    private static void ValidatePricingOverride(CustomerPricingOverrideDto? dto)
    {
        if (dto is null) return;

        if (!dto.Price.HasValue)
            throw new DomainException("Error.PriceOverrideRequired");
        if (dto.Price.Value < 0)
            throw new DomainException("Error.PriceOverrideNegative");

        if (!dto.FixedCharge.HasValue)
            throw new DomainException("Error.FixedChargeOverrideRequired");
        if (dto.FixedCharge.Value < 0)
            throw new DomainException("Error.FixedChargeOverrideNegative");

        if (!dto.TVA.HasValue)
            throw new DomainException("Error.TvaOverrideRequired");
        if (dto.TVA.Value < 0 || dto.TVA.Value > 100)
            throw new DomainException("Error.TvaOverrideRange");
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
            ?? throw new DomainException("Error.BoxNotFound");
    }

    private async Task EnsureAmpereScheduleExistsAsync(Guid? ampereScheduleId)
    {
        if (ampereScheduleId is null) return;

        _ = await _ampereScheduleRepository.GetByIdAsync(ampereScheduleId.Value)
            ?? throw new DomainException("Error.ScheduleNotFound");
    }

    private async Task<AppPreferences> GetPreferencesAsync()
        => await _preferencesRepository.GetAsync()
            ?? throw new DomainException("Error.PreferencesNotConfigured");
}
