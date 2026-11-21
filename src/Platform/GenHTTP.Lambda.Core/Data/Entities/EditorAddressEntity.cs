namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// An address a lambda was changed from with its editor key.
/// </summary>
/// <remarks>
/// Kept so that the operator who blocks a lambda knows where it was made and
/// can block that as well. One row per lambda and address, moved on as the
/// address is seen again and pruned once it has not been for a while (see
/// <c>EditorAddresses</c>). Never an address of the installation's own
/// networks, which the build agent and a proxy in front reach it from.
/// </remarks>
public sealed class EditorAddressEntity
{

    public long LambdaId { get; set; }

    /// <summary>
    /// The address as the connection came from it, an IPv4 client on a dual
    /// stack socket written as IPv4.
    /// </summary>
    public required string Address { get; set; }

    /// <summary>
    /// When the lambda was first changed from it.
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// When it was last changed from it, to within an hour.
    /// </summary>
    public DateTime LastSeen { get; set; }

}
