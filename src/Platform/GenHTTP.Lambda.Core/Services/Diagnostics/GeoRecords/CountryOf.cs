using MaxMind.Db;

namespace GenHTTP.Lambda.Services.Diagnostics;

internal sealed class CountryOf
{
    [MapKey("iso_code")]
    public string? IsoCode { get; set; }
}
