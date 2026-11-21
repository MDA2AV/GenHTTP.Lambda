namespace GenHTTP.Lambda.Services.Data;

/// <summary>
/// One kind of data a lambda can keep.
/// </summary>
/// <param name="Id">How the kind is named in the API, the database and the editor</param>
/// <param name="Default">Whether a lambda has it until its owner says otherwise</param>
public sealed record DataKind(string Id, bool Default);
