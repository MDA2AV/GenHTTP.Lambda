namespace GenHTTP.Lambda.Services.Diagnostics;

/// <summary>
/// What a database says about one address.
/// </summary>
/// <param name="Network">
/// The organisation the range belongs to - an ISP for a person, a hosting
/// company or a cloud for everything else, which is often the more telling of
/// the two.
/// </param>
public sealed record Place(string? City, string? Region, string? Country, string? Network)
{

    /// <summary>
    /// One line of it, for a log that has one column to spend.
    /// </summary>
    public string Describe()
    {
        var where = string.Join(", ", new[] { City, Region, Country }.Where(p => !string.IsNullOrEmpty(p)).Distinct());

        if (string.IsNullOrEmpty(Network))
        {
            return where;
        }

        return where.Length == 0 ? Network : $"{where} · {Network}";
    }

}
