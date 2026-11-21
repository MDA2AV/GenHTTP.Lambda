namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Raised by the services for problems the caller can act on.
/// </summary>
public sealed class LambdaException(LambdaError error, string message) : Exception(message)
{

    public LambdaError Error { get; } = error;

    public static LambdaException NotFound(string message) => new(LambdaError.NotFound, message);

    public static LambdaException Conflict(string message) => new(LambdaError.Conflict, message);

    public static LambdaException Invalid(string message) => new(LambdaError.Invalid, message);

    public static LambdaException Forbidden(string message) => new(LambdaError.Forbidden, message);

    public static LambdaException TooMany(string message) => new(LambdaError.TooMany, message);

    public static LambdaException Blocked(string message) => new(LambdaError.Blocked, message);

}
