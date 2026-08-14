namespace Shabakat.Application.Contracts.Services;

public interface IDatabaseSeedService
{
    Task<bool> IsSeededAsync(CancellationToken cancellationToken = default);

    IAsyncEnumerable<double> SeedAsync(CancellationToken cancellationToken = default);
}
