using Microsoft.Maui.Networking;
using Shabakat.Application.Contracts.Services;

namespace Shabakat.Application.Services.Market;

public sealed class MauiMarketConnectivity : IMarketConnectivity, IDisposable
{
    public event EventHandler<bool>? ConnectivityChanged;

    public bool HasInternetAccess =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public MauiMarketConnectivity()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e) =>
        ConnectivityChanged?.Invoke(this, e.NetworkAccess == NetworkAccess.Internet);

    public void Dispose() =>
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChanged;
}
