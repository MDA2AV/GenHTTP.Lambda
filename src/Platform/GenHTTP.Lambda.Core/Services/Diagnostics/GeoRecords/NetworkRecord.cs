using System.Net;

using MaxMind.Db;

namespace GenHTTP.Lambda.Services.Diagnostics;

internal sealed class NetworkRecord
{
    [MapKey("autonomous_system_organization")]
    public string? Organisation { get; set; }
}
