using System.Text.RegularExpressions;

namespace Hammer.Auction.Application.Common;

/// <summary>
///     Parses Korean land lot addresses (지번주소) to extract location components.
/// </summary>
internal static partial class AddressParser
{
    /// <summary>
    ///     Extracts the district name (읍면동/리) and lot number (지번) from a land lot address.
    /// </summary>
    /// <returns>A tuple of (UmdNm, Jibun). Both null if parsing fails.</returns>
    public static (string? UmdNm, string? Jibun) ParseLocation(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return (null, null);

        Match match = LocationPattern().Match(address);

        if (!match.Success)
            return (null, null);

        var jibun = match.Groups["jibun"].Value.Replace(" ", string.Empty, StringComparison.Ordinal);

        return (match.Groups["umd"].Value, jibun);
    }

    /// <summary>
    ///     Parses a collection of addresses and returns a dictionary mapping each address to its parsed (UmdNm, Jibun).
    ///     Addresses that fail to parse are excluded.
    /// </summary>
    public static Dictionary<string, (string UmdNm, string Jibun)> BuildLocationMap(
        IEnumerable<string> addresses)
    {
        ArgumentNullException.ThrowIfNull(addresses);

        return addresses
            .Distinct()
            .Select(addr => (Address: addr, Parsed: ParseLocation(addr)))
            .Where(x => x.Parsed.UmdNm is not null && x.Parsed.Jibun is not null)
            .ToDictionary(x => x.Address, x => (x.Parsed.UmdNm!, x.Parsed.Jibun!));
    }

    /// <summary>
    ///     Reverse address to location.
    /// </summary>
    /// <param name="addressToLocation"></param>
    /// <returns>Reverse dictionary.</returns>
    public static Dictionary<(string UmdNm, string Jibun), List<string>> Reverse(Dictionary<string, (string UmdNm, string Jibun)> addressToLocation) =>
        addressToLocation
            .GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Select(kv => kv.Key).ToList());

    [GeneratedRegex(@"(?<umd>\S+[동리가])\s+(?<jibun>산\s*\d[\d-]*|\d[\d-]*)")]
    private static partial Regex LocationPattern();
}
