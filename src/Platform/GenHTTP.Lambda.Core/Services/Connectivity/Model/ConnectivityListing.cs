namespace GenHTTP.Lambda.Services.Connectivity;

/// <summary>
/// A lambda whose connectivity the operator decides, or has decided, as the panel
/// lists it.
/// </summary>
/// <param name="ActiveVersion">The version online, if one is</param>
/// <param name="Blocked">Since when the operator has blocked it, if they have</param>
/// <param name="Connects">Whether the code of its version online, or of its newest, connects to other servers, as far as what is written tells</param>
public sealed record ConnectivityListing(string PublicKey, string Tier, int? ActiveVersion, DateTime? Blocked, ConnectivityInfo Connectivity, bool Connects);
