using Shabakat.Application.Contracts.Services;

namespace Shabakat.Application.Services.Toasts;

public sealed class ToastService : IToastService
{
    private readonly List<ToastMessage> _messages = [];
    private readonly object _gate = new();

    public event Action? OnChanged;

    public IReadOnlyList<ToastMessage> Messages
    {
        get
        {
            lock (_gate)
                return _messages.ToList();
        }
    }

    public void Success(string message) => Add(message, ToastVariant.Success);

    public void Error(string message) => Add(message, ToastVariant.Error);

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
