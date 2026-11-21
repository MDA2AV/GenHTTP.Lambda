namespace GenHTTP.Lambda.Services.UsageReports;

/// <param name="Days">How far back this looks</param>
/// <param name="Versions">
/// The versions saved in that time by where they came from: <c>mcp</c> (an agent over MCP), <c>api</c> (the REST API and
/// the editor), <c>git</c> (a push), <c>template</c> (a new lambda) and <c>other</c>
/// </param>
/// <param name="BuildAgent">
/// Of those, how many the build agent of the installation wrote - through MCP, or as a draft it wrote that was put
/// online; counted under <c>mcp</c> or <c>api</c> above as well
/// </param>
/// <param name="Models">The versions the build agent wrote, by the model it ran on: every model the catalog has, by name</param>
/// <param name="Efforts">The versions the build agent wrote, by the effort it ran at: every effort, by name</param>
public sealed record UsageActivity(int Days, IReadOnlyDictionary<string, int> Versions, int BuildAgent, IReadOnlyDictionary<string, int> Models,
                                   IReadOnlyDictionary<string, int> Efforts);
