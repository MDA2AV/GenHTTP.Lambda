using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Meta.Model;

namespace GenHTTP.Lambda.Services.Visits;

/// <summary>
/// The client hints a Chromium browser sends with every request by default:
/// its brands, whether it is on a phone, and its operating system.
/// </summary>
/// <remarks>
/// Chromium froze most of its User-Agent - every Android phone is "Android 10;
/// K" there - and says the rest here instead, so where they are sent they are
/// the better answer. Kept as sent; like the agent, they are written by the
/// sender and read with that in mind.
/// </remarks>
/// <param name="Brands">Sec-CH-UA, such as <c>"Google Chrome";v="141", "Not?A_Brand";v="8", "Chromium";v="141"</c></param>
/// <param name="Mobile">Sec-CH-UA-Mobile, <c>?1</c> on a phone</param>
/// <param name="Platform">Sec-CH-UA-Platform, such as <c>"Android"</c></param>
public sealed record ClientHints(string? Brands, string? Mobile, string? Platform);
