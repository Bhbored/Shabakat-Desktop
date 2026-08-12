namespace Shabakat.Domain.Exceptions;

/// <summary>
/// Expected business failure. Message is safe to show in a toast.
/// Unexpected bugs should not use this — let them bubble and show a generic error.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message)
        : base(message) { }

    public DomainException(string message, Exception inner)
        : base(message, inner) { }
}
