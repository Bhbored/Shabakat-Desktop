using Shabakat.Domain.Common;
using Shabakat.Domain.Enums;

namespace Shabakat.Domain.Entities;

public class Expenses : Base
{
    public ExpenseType ExpenseType { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Label { get; set; }
    public string? Notes { get; set; }
}
