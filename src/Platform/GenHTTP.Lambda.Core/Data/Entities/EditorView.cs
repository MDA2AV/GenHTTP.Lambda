namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// How the editor of a lambda opens for somebody who has not chosen for
/// themselves.
/// </summary>
/// <remarks>
/// A default rather than a rule: whoever opens the editor can switch, and the
/// switch is remembered in their browser, never here - so an administrator
/// looking at somebody's lambda in the full view leaves it simple for its
/// owner. The name is what is stored, as with the tier.
/// </remarks>
public enum EditorView
{

    /// <summary>
    /// Every section: the code, the files, the data, the versions, the
    /// deployments and the logs. For somebody who writes the code, or
    /// wants to see it.
    /// </summary>
    Full,

    /// <summary>
    /// The app, how it is doing and a box to ask for a change - for
    /// somebody who had it built from a sentence and does not want to know
    /// what a version or a deployment is.
    /// </summary>
    Simple

}
