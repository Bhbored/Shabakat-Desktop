using Microsoft.Extensions.Localization;
using Shabakat.Application.Contracts.Services;
using Shabakat.Application.Helper;
using Shabakat.Domain.Exceptions;
using Shabakat.Resources.Localization;

namespace Shabakat.Application.Services.Toasts;

public sealed class ToastService : IToastService
{
    private readonly List<ToastMessage> _messages = [];
    private readonly object _gate = new();
    private readonly IStringLocalizer<SharedResource> _localizer;

    public event Action? OnChanged;

    public ToastService(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    public IReadOnlyList<ToastMessage> Messages
    {
        get
        {
            lock (_gate)
                return _messages.ToList();
        }
    }

    public void Success(string message) => Add(message, ToastVariant.Success);

    public void Error(string message, params object[] args)
        => Add(ToastLocalizer.Resolve(_localizer, message, args), ToastVariant.Error);

    public void Error(Exception exception)
    {
        if (exception is DomainException domain)
            Error(domain.Message, domain.Args);
        else
            Error("Error.Unexpected");
    }

    public void Info(string message) => Add(message, ToastVariant.Info);

    public void Warning(string message) => Add(message, ToastVariant.Warning);

    public void Dismiss(Guid id)
    {
        lock (_gate)
            _messages.RemoveAll(m => m.Id == id);

        OnChanged?.Invoke();
    }

    public void Clear()
    {
        lock (_gate)
            _messages.Clear();

        OnChanged?.Invoke();
    }

    private void Add(string message, ToastVariant variant)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var toast = new ToastMessage(
            Id: Guid.NewGuid(),
            Text: message.Trim(),
            Variant: variant,
            CreatedAt: DateTime.Now);

        lock (_gate)
        {
            _messages.Add(toast);
            if (_messages.Count > 5)
                _messages.RemoveAt(0);
        }

        OnChanged?.Invoke();
    }
}
