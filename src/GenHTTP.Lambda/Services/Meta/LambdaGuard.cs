using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Who may change a lambda: whoever holds its editor key, unless it is a
/// demo, which only the installation changes.
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
    /// Refuses a change to a demo unless the installation itself is making it.
    /// </summary>
    /// <remarks>
    /// The editor key of a demo is announced so that anybody can read it, which
    /// makes holding the key mean nothing about being allowed to change it.
    /// Checked in the services rather than in front of the API, so that no
    /// door into a lambda - the editor, the REST API or an agent - can forget it.
    /// </remarks>
    /// <param name="origin">Who is asking: the seeder and the operator may, nobody else</param>
    public static void EnsureEditable(LambdaEntity lambda, string? origin = null)
    {
        if (lambda.Tier == LambdaTier.Demo && origin is not (VersionOrigins.System or VersionOrigins.Admin))
        {
            throw LambdaException.Forbidden(ReadOnly(lambda.PublicKey));
        }
    }

    /// <summary>
    /// What somebody is told who tries to change a demo, which is also what
    /// to do instead.
    /// </summary>
    public static string ReadOnly(string publicKey)
        => $"'{publicKey}' is a demo and read only: read its code, files and logs as much as you like. " +
           $"To change it, create a lambda of your own from it - create_lambda (POST /api/v1/lambdas) with template '{publicKey}'.";

}
