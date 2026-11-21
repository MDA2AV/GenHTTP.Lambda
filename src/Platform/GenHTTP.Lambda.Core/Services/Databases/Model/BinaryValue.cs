namespace GenHTTP.Lambda.Services.Databases;

/// <summary>
/// A value of bytes rather than text, told by its length.
/// </summary>
public sealed record BinaryValue(long Blob);
