using Shabakat.Domain.Common;

namespace Shabakat.Domain.Entities;

public class AppUser : Base
{
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset LicensedUntil { get; set; }
    public string LicenseStamp { get; set; } = string.Empty;
    public string? BusinessName { get; set; }
    public string? LogoUrl { get; set; }
}
