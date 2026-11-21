namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// What a tier is called wherever it leaves the platform - the API, MCP, the
/// pages and the settings it is saved in - spelled out here so the names
/// never follow a rename in code.
/// </summary>
public static class LambdaTierNames
{

    public static string Name(LambdaTier tier) => tier switch
    {
        LambdaTier.Sandbox => "Sandbox",
        LambdaTier.Production => "Production",
        LambdaTier.System => "System",
        LambdaTier.Demo => "Demo",
        _ => "Sandbox"
    };

    /// <summary>
    /// The tier of a name, in any case.
    /// </summary>
    public static bool TryRead(string? name, out LambdaTier tier)
    {
        foreach (var known in Enum.GetValues<LambdaTier>())
        {
            if (string.Equals(name?.Trim(), Name(known), StringComparison.OrdinalIgnoreCase))
            {
                tier = known;
                return true;
            }
        }

        tier = default;
        return false;
    }

}
