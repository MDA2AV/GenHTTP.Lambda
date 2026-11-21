namespace GenHTTP.Lambda.Services.Building;

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
