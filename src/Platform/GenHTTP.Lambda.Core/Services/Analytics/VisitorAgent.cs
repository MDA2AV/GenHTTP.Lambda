using System.Collections.Concurrent;
using System.Text.RegularExpressions;

using GenHTTP.Lambda.Services.Visits;

namespace GenHTTP.Lambda.Services.Analytics;

/// <summary>
/// What a visitor used.
/// </summary>
/// <param name="Device">desktop, mobile, tablet, bot or other</param>
/// <param name="Browser">The browser of a person, or nothing for anything else</param>
/// <param name="System">The operating system of a person, or nothing for anything else</param>
/// <param name="Crawler">
/// Whose crawler it is - an operator that was confirmed, or "unconfirmed" - or what kind it is, for a bot; nothing for
/// a person
/// </param>
public sealed record VisitorAgent(string Device, string? Browser, string? System, string? Crawler);
