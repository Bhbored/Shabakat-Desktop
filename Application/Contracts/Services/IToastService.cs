namespace Shabakat.Application.Contracts.Services;

public enum ToastVariant
{
    Success,
    Error,
    Info,
    Warning
}

public sealed record ToastMessage(
    Guid Id,
    string Text,
    ToastVariant Variant,
    DateTime CreatedAt);

public interface IToastService
{
    event Action? OnChanged;
    IReadOnlyList<ToastMessage> Messages { get; }
    void Success(string message);
    void Error(string message, params object[] args);
    void Error(Exception exception);
    void Info(string message);
    void Warning(string message);
    void Dismiss(Guid id);
    void Clear();
}
