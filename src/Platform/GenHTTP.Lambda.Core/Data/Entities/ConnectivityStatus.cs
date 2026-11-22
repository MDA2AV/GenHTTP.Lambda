namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Where the owner's request for a lambda to connect to other servers stands.
/// </summary>
/// <remarks>
/// The request alone: whether the code may connect is this and the tier
/// together (see <c>ConnectivityRule</c>), since a decision comes before what the tier allows by default, whatever is
/// written here. Stored as its number, as the tier is, and named by
/// <see cref="ConnectivityStatusNames"/>; a number no member has reads as
/// nothing asked or decided.
/// </remarks>
public enum ConnectivityStatus
{

    /// <summary>
    /// The owner asked, and the operator has not decided yet.
    /// </summary>
    Requested = 0,

    /// <summary>
    /// The operator allowed it - asked for or not.
    /// </summary>
    Allowed = 1,

    /// <summary>
    /// The operator turned the request down, or took back what they had
    /// allowed. The owner may ask again.
    /// </summary>
    Declined = 2

}
