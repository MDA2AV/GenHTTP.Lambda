using MaxMind.Db;

namespace GenHTTP.Lambda.Services.Diagnostics;

internal sealed class CityRecord
{
    [MapKey("city")]
    public NamedPlace? City { get; set; }

    [MapKey("country")]
    public CountryOf? Country { get; set; }

    [MapKey("subdivisions")]
    public List<NamedPlace>? Subdivisions { get; set; }
}
