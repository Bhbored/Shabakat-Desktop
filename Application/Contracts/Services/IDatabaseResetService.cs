namespace Shabakat.Application.Contracts.Services;

public interface IDatabaseResetService
{
    Task ResetAsync(CancellationToken cancellationToken = default);
}
