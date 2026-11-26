namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The words a person reads.
/// </summary>
public enum AgentView
{

    /// <summary>
    /// Somebody who does not write code: no developer words - a feature is a
    /// draft, merging is putting it online, the database is their records,
    /// secrets are keys and passwords, the workspace is what the app saved -
    /// and no files, logs or compile errors, which are the agent's to deal
    /// with.
    /// </summary>
    Simple = 0,

    /// <summary>
    /// A developer: the precise words - features, versions, merging, the
    /// database, secrets, files.
    /// </summary>
    Full = 1

}
