namespace Shabakat.Application.Contracts.Services;

public interface IMarketConnectivity
{
    bool HasInternetAccess { get; }
    event EventHandler<bool>? ConnectivityChanged;
}
