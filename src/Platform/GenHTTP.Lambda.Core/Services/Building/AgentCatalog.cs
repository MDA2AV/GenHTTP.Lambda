namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// The models the build agent may run on.
/// </summary>
/// <remarks>
/// Which of them an installation offers, and to whom, is its operator's
/// (<see cref="AgentSettings"/>); which model of Anthropic's each runs as is
/// the runner's, which maps the names to identifiers of its own.
/// </remarks>
public enum AgentModel
{
    Opus,
    Sonnet,
    Haiku,
    Fable
}

/// <summary>
/// How much the agent thinks and how many steps it takes over a job - the
/// effort the model is asked for.
/// </summary>
public enum AgentEffort
{
    Low,
    Medium,
    High,
    ExtraHigh,
    Max
}

/// <summary>
/// Who may have a model of the build agent.
/// </summary>
public enum ModelAccess
{
    /// <summary>Nobody: it is not offered.</summary>
    Off,

    /// <summary>Everybody who may ask the agent at all.</summary>
    Open,

    /// <summary>
    /// Whoever knows the installation's password - and runs without a clock
    /// and without a limit on its steps, since it was asked for by somebody
    /// the operator trusts with that.
    /// </summary>
    Password
}

/// <summary>
/// What the models and efforts are called: on the wire, in the settings, and
/// on the pages.
/// </summary>
/// <remarks>
/// The names are what pages, the API and the runner say; the labels are what
/// the pages show, the version included, so they name the models the runner
/// runs by default. An operator who points the runner at other identifiers
/// sees them in the panel beside the labels.
/// </remarks>
public static class AgentCatalog
{

    /// <summary>One model as the platform knows it.</summary>
    /// <param name="Name">What the API, the settings and the runner call it</param>
    /// <param name="Label">What the pages show</param>
    public sealed record Entry(AgentModel Model, string Name, string Label);

    /// <summary>
    /// Every model, in the order the pages list them: the default first, the
    /// one that was behind a password from the start last.
    /// </summary>
    public static readonly IReadOnlyList<Entry> Models =
    [
        new(AgentModel.Opus, "opus", "Opus 5.5"),
        new(AgentModel.Sonnet, "sonnet", "Sonnet 5.5"),
        new(AgentModel.Haiku, "haiku", "Haiku 5.5"),
        new(AgentModel.Fable, "fable", "Fable 5.2")
    ];

    /// <summary>Every effort, from the least to the most, by the name the runner hands the CLI.</summary>
    public static readonly IReadOnlyList<(AgentEffort Effort, string Name)> Efforts =
    [
        (AgentEffort.Low, "low"),
        (AgentEffort.Medium, "medium"),
        (AgentEffort.High, "high"),
        (AgentEffort.ExtraHigh, "xhigh"),
        (AgentEffort.Max, "max")
    ];

    public static string NameOf(AgentModel model) => Models.First(m => m.Model == model).Name;

    public static string LabelOf(AgentModel model) => Models.First(m => m.Model == model).Label;

    public static string NameOf(AgentEffort effort) => Efforts.First(e => e.Effort == effort).Name;

    public static string NameOf(ModelAccess access) => access switch
    {
        ModelAccess.Open => "open",
        ModelAccess.Password => "password",
        _ => "off"
    };

    /// <summary>The model of a name, however it is spaced or cased.</summary>
    public static bool TryModel(string? name, out AgentModel model)
    {
        var wanted = Normalize(name);

        foreach (var entry in Models)
        {
            if (entry.Name == wanted)
            {
                model = entry.Model;
                return true;
            }
        }

        model = default;
        return false;
    }

    /// <summary>The effort of a name, however it is spaced or cased.</summary>
    public static bool TryEffort(string? name, out AgentEffort effort)
    {
        var wanted = Normalize(name);

        foreach (var (known, spelled) in Efforts)
        {
            if (spelled == wanted)
            {
                effort = known;
                return true;
            }
        }

        effort = default;
        return false;
    }

    /// <summary>Who may have a model, by the name the settings keep it under.</summary>
    public static bool TryAccess(string? name, out ModelAccess access)
    {
        switch (Normalize(name))
        {
            case "open":
                access = ModelAccess.Open;
                return true;
            case "password":
                access = ModelAccess.Password;
                return true;
            case "off":
                access = ModelAccess.Off;
                return true;
            default:
                access = default;
                return false;
        }
    }

    private static string Normalize(string? name) => (name ?? "").Trim().ToLowerInvariant();

}
