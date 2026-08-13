using Shabakat.Application.Contracts.Services;
using Shabakat.Domain.Exceptions;

namespace Shabakat.Application.Helper;

public static class UiActions
{
    public static async Task RunAsync(
        IToastService toast,
        Func<Task> action,
        string? successMessage = null)
    {
        try
        {
            await action();
            if (!string.IsNullOrWhiteSpace(successMessage))
                toast.Success(successMessage);
        }
        catch (DomainException ex)
        {
            toast.Error(ex.Message);
        }
    }

    public static async Task<T?> RunAsync<T>(
        IToastService toast,
        Func<Task<T>> action,
        string? successMessage = null)
    {
        try
        {
            var result = await action();
            if (!string.IsNullOrWhiteSpace(successMessage))
                toast.Success(successMessage);
            return result;
        }
        catch (DomainException ex)
        {
            toast.Error(ex.Message);
            return default;
        }
    }
}
