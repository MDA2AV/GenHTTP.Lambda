using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <remarks>
/// None of the counts includes the demos, which every installation has.
/// </remarks>
/// <param name="Total">Every lambda</param>
/// <param name="Free">Of those, in the sandbox - what the tier was called before, which the collector reads</param>
/// <param name="Premium">In production - the same, under its name from before</param>
/// <param name="Online">With a version online</param>
/// <param name="Blocked">Blocked by the operator</param>
/// <param name="Created30Days">Created in the last 30 days</param>
/// <param name="Active30Days">Changed or visited in the last 30 days</param>
/// <param name="SimpleView">Opening in the simple view of the editor, which is how the build agent makes them</param>
/// <param name="Versions">Their versions, all together</param>
/// <param name="Features">Their features (drafts), all together</param>
/// <param name="Sandbox">In the sandbox</param>
/// <param name="Production">In production</param>
/// <param name="System">In the system tier, the installation's own</param>
public sealed record UsageLambdas(int Total, int Free, int Premium, int Online, int Blocked, int Created30Days, int Active30Days,
                                  int SimpleView, int Versions, int Features, int Sandbox, int Production, int System);
