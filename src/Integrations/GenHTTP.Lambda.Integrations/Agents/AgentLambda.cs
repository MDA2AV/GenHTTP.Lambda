namespace GenHTTP.Lambda.Integrations.Agents;

/// <summary>
/// The app a run works on.
/// </summary>
/// <remarks>
/// The editor key is what lets the agent do anything to the app, as it lets
/// its owner; the agent may create other apps as well, and has the keys of
/// no others. It is never logged and never shown to anybody but the owner.
/// </remarks>
/// <param name="PublicKey">What the app is known by in its address</param>
/// <param name="EditorKey">The key that opens it, which every door takes as privateKey</param>
/// <param name="Online">The version online as the run was asked for, if one is</param>
/// <param name="Draft">The draft (feature) to go on with, by its key, where the owner picked one; otherwise the agent starts one where it changes the app</param>
/// <param name="PutOnline">Whether to put the change online once it works, rather than leave it in its draft for the owner to try</param>
public sealed record AgentLambda(string PublicKey, string EditorKey, int? Online, string? Draft, bool PutOnline)
{

    /// <summary>
    /// Leaves the editor key out, should a record be printed.
    /// </summary>
    public override string ToString() => $"AgentLambda {{ PublicKey = {PublicKey}, Online = {Online}, Draft = {Draft}, PutOnline = {PutOnline} }}";

}
