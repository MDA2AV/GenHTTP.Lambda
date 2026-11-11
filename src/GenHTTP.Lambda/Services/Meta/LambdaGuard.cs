using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Who may change a lambda: whoever holds its editor key, unless it is a
/// demo, which only the installation changes, or the operator blocked it,
/// which nobody changes until the block is lifted.
/// </summary>
internal static class LambdaGuard
{

    /// <summary>
    /// The lambda the editor key opens, to be changed in this context.
    /// </summary>
    public static LambdaEntity Require(LambdaDbContext database, string privateKey)
        => database.Lambdas.FirstOrDefault(l => l.PrivateKey == privateKey)
        ?? throw LambdaException.NotFound("This lambda does not exist (or has been deleted).");

    /// <summary>
    /// Refuses a change to a blocked lambda, and to a demo unless the
    /// installation itself is making it.
    /// </summary>
    /// <remarks>
    /// The editor key of a demo is announced so that anybody can read it, which
    /// makes holding the key mean nothing about being allowed to change it.
    /// Checked in the services rather than in front of the API, so that no
    /// door into a lambda - the editor, the REST API or an agent - can forget it.
    /// </remarks>
    /// <param name="origin">Who is asking: the seeder and the operator may change a demo, nobody else</param>
    public static void EnsureEditable(LambdaEntity lambda, string? origin = null)
    {
        EnsureUnblocked(lambda);

        if (lambda.Tier == LambdaTier.Demo && origin is not (VersionOrigins.System or VersionOrigins.Admin))
        {
            throw LambdaException.Forbidden(ReadOnly(lambda.PublicKey));
        }
    }

    /// <summary>
    /// Refuses a change to a lambda the operator blocked, whoever makes it.
    /// </summary>
    /// <remarks>
    /// The operator too: what they may do to a blocked lambda is lift the
    /// block or delete it, and either is asked for as such. Anything else
    /// done while it is blocked - a deployment from the panel included -
    /// would put back online what it was blocked for.
    /// </remarks>
    public static void EnsureUnblocked(LambdaEntity lambda)
    {
        if (lambda.Blocked != null)
        {
            throw LambdaException.Blocked(Blocked(lambda.PublicKey));
        }
    }

    /// <summary>
    /// What somebody is told who tries to change a demo, which is also what
    /// to do instead.
    /// </summary>
    public static string ReadOnly(string publicKey)
        => $"'{publicKey}' is a demo and read only: read its code, files and logs as much as you like. " +
           $"To change it, create a lambda of your own from it - create_lambda (POST /api/v1/lambdas) with template '{publicKey}'.";

    /// <summary>
    /// What somebody is told who tries to change a blocked lambda - an owner,
    /// or their agent, which should stop rather than try again.
    /// </summary>
    /// <remarks>
    /// Not why: the owner reads the operator's note where they read the
    /// lambda - its editor, <c>blockReason</c> - rather than in every refusal,
    /// which may end up in a build log others read.
    /// </remarks>
    public static string Blocked(string publicKey)
        => $"'{publicKey}' has been blocked by the operator of this installation: it is offline, and it cannot be changed, put online or deleted any more - " +
           "through the editor, the API, MCP or git alike. What it has can still be read and exported. Trying again will not help; only the operator can lift the block.";

}
