namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// Whether a lambda has a kind of data, as the owner left it.
/// </summary>
/// <param name="Changed">When the owner last switched it, or nothing if it is as it was by default</param>
public sealed record DataSwitch(bool Enabled, DateTime? Changed);
