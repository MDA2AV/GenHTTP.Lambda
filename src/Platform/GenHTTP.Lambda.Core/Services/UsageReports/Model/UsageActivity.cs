using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="Days">How far back this looks</param>
/// <param name="Versions">
/// The versions saved in that time by where they came from: <c>mcp</c> (an agent over MCP), <c>api</c> (the REST API and
/// the editor), <c>git</c> (a push), <c>template</c> (a new lambda) and <c>other</c>
/// </param>
public sealed record UsageActivity(int Days, IReadOnlyDictionary<string, int> Versions);
