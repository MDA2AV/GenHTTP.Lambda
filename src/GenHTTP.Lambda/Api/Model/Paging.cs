using GenHTTP.Api.Content.Services;

namespace GenHTTP.Lambda.Api.Model;

[Summary("A page of a listing that is read a page at a time, with `skip` and `take`.")]
public sealed record Page<T>(
    [Summary("The entries of this page")]
    IReadOnlyList<T> Entries,
    [Summary("How many entries there are altogether")]
    int Total,
    [Summary("The `skip` of the next page; absent when this was the last")]
    int? Next
)
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
