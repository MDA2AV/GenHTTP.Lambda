using System.Text.Json.Serialization;

namespace GenHTTP.Lambda.Services.UsageReports;

/// <summary>
/// How many lambdas use each of the platform's features.
/// </summary>
/// <param name="Database">With a database</param>
/// <param name="Workspace">With a workspace</param>
/// <param name="Secrets">With secrets</param>
/// <param name="Analytics">Counting their visits</param>
/// <param name="Showcase">In the showcase</param>
/// <param name="OpenSource">With their source published</param>
/// <param name="Domain">With a domain of their own</param>
/// <param name="ConnectivityAllowed">Allowed by the operator to connect to other servers</param>
/// <param name="ConnectivityRequested">Waiting for the operator to decide that</param>
/// <param name="Git">Whose repository was read with git at least once</param>
public sealed record UsageSwitches(int Database, int Workspace, int Secrets, int Analytics, int Showcase, int OpenSource, int Domain,
                                   int ConnectivityAllowed, int ConnectivityRequested, int Git);
