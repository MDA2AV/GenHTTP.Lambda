using MaxMind.Db;

namespace GenHTTP.Lambda.Services.Diagnostics;

internal sealed class NamedPlace
{
    [MapKey("names")]
    public Names? Names { get; set; }
}
