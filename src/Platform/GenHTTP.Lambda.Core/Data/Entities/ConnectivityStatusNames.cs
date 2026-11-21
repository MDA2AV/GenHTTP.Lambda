namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What the state of a request to connect is called wherever it leaves the
/// platform.
/// </summary>
public static class ConnectivityStatusNames
{

    public static string? Name(ConnectivityStatus? status) => status switch
    {
        ConnectivityStatus.Requested => "Requested",
        ConnectivityStatus.Allowed => "Allowed",
        ConnectivityStatus.Declined => "Declined",
        _ => null
    };

    /// <summary>
    /// The state of a name, in any case.
    /// </summary>
    public static bool TryRead(string? name, out ConnectivityStatus status)
    {
        foreach (var known in Enum.GetValues<ConnectivityStatus>())
        {
            if (string.Equals(name?.Trim(), Name(known), StringComparison.OrdinalIgnoreCase))
            {
                status = known;
                return true;
            }
        }

        status = default;
        return false;
    }

}
