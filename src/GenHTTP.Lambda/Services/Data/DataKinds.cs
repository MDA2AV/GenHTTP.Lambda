namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// One kind of data a lambda can keep.
/// </summary>
/// <param name="Id">How the kind is named in the API, the database and the editor</param>
/// <param name="Default">Whether a lambda has it until its owner says otherwise</param>
public sealed record DataKind(string Id, bool Default);

/// <summary>
/// The kinds of data there are.
/// </summary>
/// <remarks>
/// A version is the program - its code and its assets - and is replaced by
/// the next one. Data is what the program keeps, and belongs to the lambda:
/// every version reads and writes the same, and none of them brings back what
/// it was. Each kind is switched on by the owner rather than assumed, because
/// each is somewhere personal data can end up, and because a kind switched off
/// holds nothing at all - switching one off deletes what it held.
///
/// The workspace is the only one so far, and on by default, since every
/// lambda always had it. A database and secrets are meant to join it here:
/// a kind is added to <see cref="All" />, and everything that lists them - the
/// API, the editor, what an agent reads - lists it too.
/// </remarks>
public static class DataKinds
{

    /// <summary>
    /// The private directory a lambda reads and writes files in.
    /// </summary>
    public const string WorkspaceId = "workspace";

    public static readonly DataKind Workspace = new(WorkspaceId, true);

    /// <summary>
    /// Every kind this installation offers, in the order they are shown.
    /// </summary>
    public static readonly IReadOnlyList<DataKind> All = [Workspace];

    /// <summary>
    /// The kind with the given name, or nothing if there is none.
    /// </summary>
    public static DataKind? Find(string? id) => All.FirstOrDefault(k => string.Equals(k.Id, id?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// The kind with the given name, or a refusal that lists the ones there are.
    /// </summary>
    public static DataKind Require(string? id)
        => Find(id) ?? throw Meta.LambdaException.NotFound($"There is no kind of data called '{id}'. There is: {string.Join(", ", All.Select(k => k.Id))}.");

    /// <summary>
    /// What a lambda, or whoever tries to put a file there, is told when its
    /// workspace is switched off.
    /// </summary>
    /// <remarks>
    /// Compiled into the lambda as well, so it is what a stack trace in the
    /// log says - read there by the owner, and by an agent reading the logs.
    /// </remarks>
    public const string WorkspaceOff =
        "The workspace of this lambda is switched off, so it cannot keep files. Its owner can switch it on under Data in the editor (PUT /api/v1/lambdas/{privateKey}/data/workspace).";

}
