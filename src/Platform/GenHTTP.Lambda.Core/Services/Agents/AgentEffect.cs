namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// One thing a run changed, with what undoing it needs to know.
/// </summary>
/// <remarks>
/// Recorded by the service that made the change, whichever door the call
/// came through, once it is made - so what a run changed is what the
/// platform did for it, not what the agent says it did. Undoing reads what
/// was there before (<see cref="Before"/>, <see cref="Was"/>), and does
/// nothing where it changed since.
/// </remarks>
/// <param name="Kind">What was changed</param>
/// <param name="LambdaId">The lambda it was changed on</param>
/// <param name="Version">The version saved, put online or merged into</param>
/// <param name="Before">What was online before, for a version put online; how the editor opened before, for a view</param>
/// <param name="Feature">The feature, by its key</param>
/// <param name="Name">The feature's name, the kind of data, the license, the path in the workspace or the view</param>
/// <param name="Was">Whether it was so before: the source published, connecting asked for, the lambda in the showcase</param>
/// <param name="Online">Whether a version saved or merged went online in the same call</param>
public sealed record AgentEffect(AgentEffectKind Kind, long LambdaId, int? Version = null, int? Before = null, string? Feature = null, string? Name = null,
                                 bool? Was = null, bool? Online = null);
