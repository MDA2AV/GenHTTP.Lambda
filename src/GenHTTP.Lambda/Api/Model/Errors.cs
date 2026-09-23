namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// How an error is reported to the single page application.
/// </summary>
public sealed record ErrorResponse(int Status, string Error, string Message);
