using System.Security.Cryptography;
using System.Text;

namespace Shabakat.Application.Helper;

public static class LicenseHmac
{
    private static readonly byte[] Key = Convert.FromBase64String(
        "hMtljsx2bAfQztx0Fhh1xwzUY+qDZp5KfdWKOSB7jhU=");

    public static string Compute(string passwordHash, DateTimeOffset licensedUntil)
    {
        var payload = Encoding.UTF8.GetBytes($"{passwordHash}|{licensedUntil.ToUnixTimeSeconds()}");
        var hash = HMACSHA256.HashData(Key, payload);
        return Convert.ToBase64String(hash);
    }

    public static bool Matches(string stamp, string passwordHash, DateTimeOffset licensedUntil)
    {
        if (string.IsNullOrWhiteSpace(stamp) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        byte[] actual;
        try
        {
            actual = Convert.FromBase64String(stamp);
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = Convert.FromBase64String(Compute(passwordHash, licensedUntil));
        return actual.Length == expected.Length
            && CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
