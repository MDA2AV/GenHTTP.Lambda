namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// One page of a listing that is read a page at a time, with <c>skip</c> and
/// <c>take</c>.
/// </summary>
/// <param name="Total">How many there are altogether</param>
/// <param name="Next">Where the next page starts, or nothing when this was the last</param>
public sealed record Page<T>(IReadOnlyList<T> Entries, int Total, int? Next)
{

    /// <summary>
    /// The page that starts after <paramref name="skip"/> entries.
    /// </summary>
    public static Page<T> Of(IReadOnlyList<T> entries, int skip, int total)
    {
        var next = skip + entries.Count;

        return new Page<T>(entries, total, next < total ? next : null);
    }

}
