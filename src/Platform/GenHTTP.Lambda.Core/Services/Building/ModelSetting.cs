namespace GenHTTP.Lambda.Services.Building;

/// <summary>
/// Who may have one model of the build agent.
/// </summary>
public sealed record ModelSetting(AgentModel Model, ModelAccess Access);
