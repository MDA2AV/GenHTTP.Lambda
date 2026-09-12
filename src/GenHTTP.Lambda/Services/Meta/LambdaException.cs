namespace GenHTTP.Lambda.Services.Meta;

/// <summary>
/// Why a request against the meta service could not be carried out. Translated
/// into HTTP status codes by the API layer.
/// </summary>
public enum LambdaError
{
    NotFound,
    Conflict,
    Invalid
}

/// <summary>
/// Raised by the services for problems the caller can act on.
/// </summary>
public sealed class LambdaException(LambdaError error, string message) : Exception(message)
{

    public LambdaError Error { get; } = error;

    public static LambdaException NotFound(string message) => new(LambdaError.NotFound, message);

    public static LambdaException Conflict(string message) => new(LambdaError.Conflict, message);

    public static LambdaException Invalid(string message) => new(LambdaError.Invalid, message);

}
