namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Why a request against the meta service could not be carried out. Translated
/// into HTTP status codes by the API layer.
/// </summary>
public enum LambdaError
{
    NotFound,
    Conflict,
    Invalid,

    /// <summary>
    /// Allowed in general, but not for this lambda - its tier does not include it.
    /// </summary>
    Forbidden,

    /// <summary>
    /// Allowed, but asked for too often lately by the same caller.
    /// </summary>
    TooMany,

    /// <summary>
    /// The operator has blocked the lambda: nothing about it may be changed.
    /// </summary>
    Blocked
}
