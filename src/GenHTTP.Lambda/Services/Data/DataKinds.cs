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
/// A version is the program - its code and its resources - and is replaced by
/// the next one. Data is what the program keeps, and belongs to the lambda:
/// every version reads and writes the same, and none of them brings back what
/// it was. Each kind is switched on by the owner rather than assumed, because
/// each is somewhere personal data can end up, and because a kind switched off
/// holds nothing at all - switching one off deletes what it held.
///
/// The workspace is on by default, since every lambda always had it. Secrets
/// and the database are off until somebody switches them on - the owner in the
/// editor, or an agent through the API or MCP when what it builds needs one.
/// A kind that comes later is added to <see cref="All" />, and everything that
/// lists them - the API, the editor, what an agent reads - lists it too.
/// </remarks>
public static class DataKinds
{

    /// <summary>
    /// The private directory a lambda reads and writes files in.
    /// </summary>
    public const string WorkspaceId = "workspace";

    /// <summary>
    /// The API keys, passwords and tokens a lambda reads and nobody sees.
    /// </summary>
    public const string SecretsId = "secrets";

    /// <summary>
    /// The SQLite database a lambda keeps its records in.
    /// </summary>
    public const string DatabaseId = "database";

    public static readonly DataKind Workspace = new(WorkspaceId, true);

    public static readonly DataKind Secrets = new(SecretsId, false);

    public static readonly DataKind Database = new(DatabaseId, false);

    /// <summary>
    /// Every kind this installation offers, in the order they are shown.
    /// </summary>
    /// <remarks>
    /// The database before the workspace: it is where records go, and records
    /// are what most lambdas keep - the workspace is for files.
    /// </remarks>
    public static readonly IReadOnlyList<DataKind> All = [Database, Workspace, Secrets];

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

    /// <summary>
    /// What a lambda that reads a secret, or whoever tries to store one, is
    /// told while its secrets are switched off.
    /// </summary>
    public const string SecretsOff =
        "The secrets of this lambda are switched off, so it has none to read. Switch them on under Data in the editor, with enable_data (MCP) or with PUT /api/v1/lambdas/{privateKey}/data/secrets, and store the value there.";

    /// <summary>
    /// What a lambda that asks for a connection is told while its database is
    /// switched off.
    /// </summary>
    public const string DatabaseOff =
        "The database of this lambda is switched off, so there is nothing to connect to. Switch it on under Data in the editor, with enable_data (MCP) or with PUT /api/v1/lambdas/{privateKey}/data/database.";

}
