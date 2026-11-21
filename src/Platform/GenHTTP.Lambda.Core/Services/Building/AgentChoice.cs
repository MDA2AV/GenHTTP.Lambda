namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// The model and the effort a job runs on, as the caller may have them.
/// </summary>
/// <param name="Unbounded">
/// Whether it runs without a clock and without a limit on its steps: only a
/// model behind the password, and only once the password was checked.
/// </param>
public sealed record AgentChoice(AgentModel Model, AgentEffort Effort, bool Unbounded)
{

    public string ModelName => AgentCatalog.NameOf(Model);

    public string EffortName => AgentCatalog.NameOf(Effort);

}
