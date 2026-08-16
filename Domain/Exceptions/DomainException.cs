namespace Shabakat.Domain.Exceptions;

public sealed class DomainException : Exception
{
    public object[] Args { get; }

    public DomainException(string message)
        : base(message)
    {
        Args = [];
    }

    public DomainException(string message, Exception inner)
        : base(message, inner)
    {
        Args = [];
    }

    private DomainException(string message, object[] args)
        : base(message)
    {
        Args = args;
    }

    public static DomainException Format(string key, params object[] args)
        => new(key, args);
}
