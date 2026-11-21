namespace GenHTTP.Lambda.Services.Meta.Model;

/// <summary>
/// An address a lambda was changed from with its editor key.
/// </summary>
/// <param name="FirstSeen">When it was first changed from there</param>
/// <param name="LastSeen">When it was last changed from there, to within an hour</param>
public sealed record EditorAddress(string Address, DateTime FirstSeen, DateTime LastSeen);
