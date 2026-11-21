using System.Data.Common;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Integrations.Databases;

/// <summary>
/// The stores the platform keeps its own records in.
/// </summary>
/// <remarks>
/// What a store is called where it is kept - a file, a schema - is the
/// provider's to decide, and stays what it was once data is kept under it.
/// </remarks>
public enum DatabaseStore
{

    /// <summary>
    /// The lambdas, their versions, features, settings and secrets.
    /// </summary>
    Platform,

    /// <summary>
    /// What was counted of the visits to lambdas, kept apart so writing a
    /// batch of counts never holds up the platform's own records.
    /// </summary>
    Analytics

}
