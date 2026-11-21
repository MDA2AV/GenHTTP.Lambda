using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// What one lambda takes of the room.
/// </summary>
/// <param name="DataBytes">Its database and its workspace, counted as its allowance counts them</param>
/// <param name="AllowedBytes">What its tier allows its data</param>
/// <param name="VersionBytes">Its versions, as they are kept</param>
/// <param name="DraftBytes">Its features (drafts), with their copies of the data</param>
/// <param name="OtherBytes">What is kept for it besides: its git history, its packed source, what it was compiled and unpacked to</param>
public sealed record LambdaStorage(string PublicKey, LambdaTier Tier, long DataBytes, long AllowedBytes, long VersionBytes, long DraftBytes, long OtherBytes)
{

    /// <summary>
    /// All of it together.
    /// </summary>
    public long TotalBytes => DataBytes + VersionBytes + DraftBytes + OtherBytes;

}
