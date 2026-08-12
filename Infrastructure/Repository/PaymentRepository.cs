using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shabakat.Application.Contracts.Repository;
using Shabakat.Domain.Entities;
using Shabakat.Infrastructure.Persistence;

namespace Shabakat.Infrastructure.Repository;

public sealed class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext db, ILoggerFactory loggerFactory) : base(db, loggerFactory) { }

    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(Guid invoiceId)
        => await _dbSet
            .Include(p => p.Customer)
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

    public async Task<IEnumerable<Payment>> GetAllWithCustomerAsync()
        => await _dbSet
            .Include(p => p.Customer)
            .OrderByDescending(p => p.PaymentDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync();
}
