using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Connectivity;
using GenHTTP.Lambda.Services.Settings;

namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// A state the listing can be narrowed to.
/// </summary>
public enum LambdaState
{

    /// <summary>A version is online.</summary>
    Online,

    /// <summary>No version is online, and it is not blocked.</summary>
    Offline,

    /// <summary>The operator blocked it.</summary>
    Blocked,

    /// <summary>
    /// A lambda of a tier that is not kept while unused which goes offline or
    /// is removed within <see cref="LambdaQuery.ExpiringWithin"/>, unless it
    /// is used before then.
    /// </summary>
    Expiring

}
