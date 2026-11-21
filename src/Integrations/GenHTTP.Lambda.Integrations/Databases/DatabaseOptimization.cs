using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// What optimizing a database did.
/// </summary>
/// <param name="Before">The room it took before, in bytes, as the provider measures the room of a database</param>
/// <param name="After">The room it takes now</param>
/// <param name="Steps">
/// What was done, as the operator would recognize it in the database's own
/// terms - the statements run, such as <c>PRAGMA optimize</c> or
/// <c>VACUUM (ANALYZE)</c> - in the order they ran; nothing where there was no
/// database
/// </param>
/// <param name="Skipped">
/// What was considered and left out, and why - such as a rebuild a database is
/// too large for - so the operator is not left wondering
/// </param>
public sealed record DatabaseOptimization(long Before, long After, IReadOnlyList<string> Steps, IReadOnlyList<string> Skipped)
{

    /// <summary>
    /// The answer for a database that is not there.
    /// </summary>
    public static readonly DatabaseOptimization None = new(0, 0, [], []);

}
