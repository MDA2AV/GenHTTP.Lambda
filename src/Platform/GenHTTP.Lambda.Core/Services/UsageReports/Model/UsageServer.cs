using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="Engine">The engine serving: <c>Kestrel</c>, <c>Ioxide</c> or <c>Internal</c>, unknown before the server runs</param>
/// <param name="GenHttp">The version of GenHTTP</param>
/// <param name="Secure">Whether the server serves TLS itself, rather than behind a proxy</param>
/// <param name="UptimeHours">How long the server has been running</param>
/// <param name="Requests">How many requests it answered in that time</param>
/// <param name="WorkingSetBytes">How much memory the process holds</param>
public sealed record UsageServer(string? Engine, [property: JsonPropertyName("genhttp")] string? GenHttp, bool Secure, double UptimeHours, long Requests,
                                 long WorkingSetBytes);
