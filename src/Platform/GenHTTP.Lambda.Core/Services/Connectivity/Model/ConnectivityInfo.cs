using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Connectivity;

/// <summary>
/// Whether a lambda may connect to other servers, and where its owner's
/// request for that stands.
/// </summary>
/// <param name="Allowed">Whether its code may connect now: its tier includes it, or the operator allowed it</param>
/// <param name="Included">Whether its tier includes it, whatever was asked</param>
/// <param name="Status">Where the request stands, or nothing while nobody asked</param>
/// <param name="Changed">Since when it stands so</param>
/// <param name="Purpose">What the owner said it connects to, and why</param>
/// <param name="Note">What the operator said deciding it</param>
public sealed record ConnectivityInfo(bool Allowed, bool Included, ConnectivityStatus? Status, DateTime? Changed, string? Purpose, string? Note)
{

    /// <summary>
    /// A lambda nobody asked anything for, in a tier that does not include it.
    /// </summary>
    public static ConnectivityInfo None { get; } = new(false, false, null, null, null, null);

}
