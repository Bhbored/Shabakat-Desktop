using System.Text.RegularExpressions;
using Shabakat.Domain.Enums;

namespace Shabakat.Application.Helper;

public static class AuditLabels
{
    public static string Action(string? action) =>
        string.IsNullOrWhiteSpace(action)
            ? "Unknown"
            : Humanize(action);

    public static string Action(AuditAction action) => Humanize(action.ToString());

    public static string Status(string? status) =>
        string.IsNullOrWhiteSpace(status) ? "Unknown" : status;

    public static string EntityType(string? entityType) =>
        string.IsNullOrWhiteSpace(entityType) ? "—" : entityType;

    private static string Humanize(string value) =>
        Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
}
