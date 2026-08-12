using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.DTOs.Expenses;
using Shabakat.Application.Helper;
using Shabakat.Domain.Entities;
using Shabakat.Domain.Enums;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Services.Expense;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseRepository _expenseRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(
        IExpenseRepository expenseRepository,
        IAuditLogService auditLogService,
        ILogger<ExpenseService> logger)
    {
        _expenseRepository = expenseRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<ExpenseListResponse> GetAllAsync(ExpenseFilterRequest filter)
    {
        var (items, totalCount) = await _expenseRepository.GetAllPagedAsync(filter);

        var allMatching = (await _expenseRepository.GetAllMatchingAsync(filter)).ToList();
        var grandTotal = allMatching.Sum(e => e.Amount);

        var pageSize = Math.Max(1, filter.PageSize);
        var pageNumber = Math.Max(1, filter.PageNumber);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ExpenseListResponse(
            Data: items.Select(MapToResponse),
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalPages: totalPages,
            HasPreviousPage: pageNumber > 1,
            HasNextPage: pageNumber < totalPages,
            TotalAmount: grandTotal);
    }

    public async Task<IEnumerable<ExpenseResponse>> GetAllUnpagedAsync()
    {
        var items = await _expenseRepository.GetAllMatchingAsync(
            new ExpenseFilterRequest());

        return items
            .OrderByDescending(e => e.ExpenseDate)
            .Select(MapToResponse);
    }

    public async Task<ExpenseResponse> GetByIdAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id)
            ?? throw new DomainException("Expense not found.");

        return MapToResponse(expense);
    }

    public async Task<ExpenseResponse> CreateAsync(CreateExpenseRequest request)
    {
        ValidateExpense(request.ExpenseType, request.Amount, request.Label);

        var expense = new Domain.Entities.Expenses
        {
            ExpenseType = request.ExpenseType,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate ?? DateOnly.FromDateTime(DateTime.Now),
            Label = request.Label?.Trim(),
            Notes = request.Notes?.Trim()
        };

        await _expenseRepository.AddAsync(expense);
        await _expenseRepository.SaveChangesAsync();

        await _auditLogService.LogSuccessAsync(AuditLogEntries.ExpenseCreated(expense));
        _logger.LogInformation(
            "Created expense {ExpenseId} ({ExpenseType}, {Amount})",
            expense.Id,
            expense.ExpenseType,
            expense.Amount);

        return MapToResponse(expense);
    }

    public async Task<ExpenseResponse> UpdateAsync(Guid id, UpdateExpenseRequest request)
    {
        var expense = await _expenseRepository.GetByIdAsync(id)
            ?? throw new DomainException("Expense not found.");

        if (request.ExpenseType is not null) expense.ExpenseType = request.ExpenseType.Value;
        if (request.Amount is not null) expense.Amount = request.Amount.Value;
        if (request.ExpenseDate is not null) expense.ExpenseDate = request.ExpenseDate.Value;
        if (request.Label is not null) expense.Label = request.Label.Trim();
        if (request.Notes is not null) expense.Notes = request.Notes.Trim();

        ValidateExpense(expense.ExpenseType, expense.Amount, expense.Label);

        _expenseRepository.Update(expense);
        await _expenseRepository.SaveChangesAsync();

        _logger.LogInformation("Updated expense {ExpenseId}", expense.Id);
        return MapToResponse(expense);
    }

    public async Task DeleteAsync(Guid id)
    {
        var expense = await _expenseRepository.GetByIdAsync(id)
            ?? throw new DomainException("Expense not found.");

        _expenseRepository.Delete(expense);
        await _expenseRepository.SaveChangesAsync();
        _logger.LogInformation("Deleted expense {ExpenseId}", id);
    }

    private static void ValidateExpense(ExpenseType type, decimal amount, string? label)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero.");

        if (type == ExpenseType.Other && string.IsNullOrWhiteSpace(label))
        {
            throw new DomainException(
                "Label is required when ExpenseType is 'Other' (e.g. 'Rent', 'Insurance').");
        }
    }

    private static ExpenseResponse MapToResponse(Domain.Entities.Expenses e) =>
        new(
            Id: e.Id,
            ExpenseType: e.ExpenseType.ToString(),
            Amount: e.Amount,
            ExpenseDate: e.ExpenseDate,
            Label: e.Label,
            Notes: e.Notes,
            CreatedAt: e.CreatedAt,
            UpdatedAt: e.UpdatedAt);
}
