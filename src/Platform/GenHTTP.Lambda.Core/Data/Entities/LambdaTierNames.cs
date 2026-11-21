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
    /// The tier a caller named, in any case - <c>Free</c> and <c>Premium</c>,
    /// what the sandbox and production were called before, read as them still,
    /// so a script written against an earlier release keeps working - or a
    /// refusal that names the tiers there are.
    /// </summary>
    public static LambdaTier Read(string tier)
    {
        var name = tier.Trim();

        if (name.Equals("Free", StringComparison.OrdinalIgnoreCase))
        {
            return LambdaTier.Sandbox;
        }

        if (name.Equals("Premium", StringComparison.OrdinalIgnoreCase))
        {
            return LambdaTier.Production;
        }

        return TryRead(name, out var read)
            ? read
            : throw Services.Meta.LambdaException.Invalid($"There is no tier called '{tier}'. There are: {string.Join(", ", Enum.GetValues<LambdaTier>().Select(Name))}.");
    }

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
