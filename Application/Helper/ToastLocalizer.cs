using Microsoft.Extensions.Localization;
using Shabakat.Domain.Exceptions;
using Shabakat.Resources.Localization;

namespace Shabakat.Application.Helper;

public static class ToastLocalizer
{
    public static string Resolve(IStringLocalizer<SharedResource> localizer, DomainException exception)
        => Resolve(localizer, exception.Message, exception.Args);

    public static string Resolve(IStringLocalizer<SharedResource> localizer, string message, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(message))
            return message;

        var formatArgs = args.Select(arg => LocalizeArg(localizer, arg)).ToArray();
        if (!IsResourceKey(message))
            return formatArgs.Length == 0 ? message : string.Format(message, formatArgs);

        var located = formatArgs.Length == 0
            ? localizer[message]
            : localizer[message, formatArgs];

        return located.ResourceNotFound ? message : located.Value;
    }

    private static object LocalizeArg(IStringLocalizer<SharedResource> localizer, object arg)
    {
        if (arg is not string text || !IsResourceKey(text))
            return arg;

        var located = localizer[text];
        return located.ResourceNotFound ? text : located.Value;
    }

    private static bool IsResourceKey(string message)
    {
        if (message.Length < 3 || !message.Contains('.'))
            return false;

        foreach (var c in message)
        {
            if (char.IsWhiteSpace(c) || !(char.IsLetterOrDigit(c) || c is '.' or '_'))
                return false;
        }

        return true;
    }
}
