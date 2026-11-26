namespace GenHTTP.Lambda.Services.Agents;

/// <summary>
/// Who may choose a value of one of the agent's options.
/// </summary>
public enum OptionAccess
{
    /// <summary>Nobody: it is not offered.</summary>
    Off,

    /// <summary>Everybody who may ask the agent at all.</summary>
    Open,

    /// <summary>
    /// Whoever knows the installation's password - and a run with it is held
    /// to the limits the operator set for such runs, none by default, since
    /// it was asked for by somebody the operator trusts with that.
    /// </summary>
    Password
}
