namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// What an access is called in the settings and on the wire.
/// </summary>
public static class OptionAccessNames
{

    public static string Name(OptionAccess access) => access switch
    {
        OptionAccess.Open => "open",
        OptionAccess.Password => "password",
        _ => "off"
    };

    /// <summary>The access of a name, however it is spaced or cased.</summary>
    public static bool TryParse(string? name, out OptionAccess access)
    {
        switch ((name ?? "").Trim().ToLowerInvariant())
        {
            case "open":
                access = OptionAccess.Open;
                return true;
            case "password":
                access = OptionAccess.Password;
                return true;
            case "off":
                access = OptionAccess.Off;
                return true;
            default:
                access = default;
                return false;
        }
    }

}
