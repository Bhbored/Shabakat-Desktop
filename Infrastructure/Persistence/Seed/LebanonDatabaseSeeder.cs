using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Shabakat.Application.Contracts.Services;

namespace Shabakat.Infrastructure.Persistence.Seed;

public sealed class LebanonDatabaseSeeder(AppDbContext db) : IDatabaseSeedService
{
    private const int StepCount = 9;

    public Task<bool> IsSeededAsync(CancellationToken cancellationToken = default)
        => db.Areas.AnyAsync(a => a.Id == SeedIds.For(SeedIds.Area, 1), cancellationToken);

    public async IAsyncEnumerable<double> SeedAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (await IsSeededAsync(cancellationToken))
        {
            yield return 1d;
            yield break;
        }

        var step = 0;

        await AddBatchAsync(LebanonSeedBuilder.Areas, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Schedules, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Boxes, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Customers, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Readings, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Invoices, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Payments, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Expenses, cancellationToken);
        yield return Progress(++step);

        await AddBatchAsync(LebanonSeedBuilder.Skips, cancellationToken);
        yield return Progress(++step);
    }

    private async Task AddBatchAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken)
        where T : class
    {
        await db.Set<T>().AddRangeAsync(entities, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        db.ChangeTracker.Clear();
    }

    private static double Progress(int step)
        => Math.Clamp(step / (double)StepCount, 0d, 1d);
}
