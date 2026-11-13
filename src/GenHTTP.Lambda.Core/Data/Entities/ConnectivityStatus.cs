namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Where the owner's request for a lambda to connect to other servers stands.
/// </summary>
/// <remarks>
/// The request alone: whether the code may connect is this and the tier
/// together (see <c>ConnectivityRule</c>), since a premium lambda may whatever is
/// written here. The name is what is stored, as with the tier.
/// </remarks>
public enum ConnectivityStatus
{

    /// <summary>
    /// The owner asked, and the operator has not decided yet.
    /// </summary>
    Requested,

    /// <summary>
    /// The operator allowed it - asked for or not.
    /// </summary>
    Allowed,

    /// <summary>
    /// The operator turned the request down, or took back what they had
    /// allowed. The owner may ask again.
    /// </summary>
    Declined

}
