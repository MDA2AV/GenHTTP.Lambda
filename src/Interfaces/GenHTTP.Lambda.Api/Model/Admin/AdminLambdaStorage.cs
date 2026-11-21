using GenHTTP.Api.Content.Documentation;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Census;

namespace GenHTTP.Lambda.Api.Model;

[Summary("What one lambda takes of the room.")]
public sealed record AdminLambdaStorage(
    [Summary("Its public key")]
    string PublicKey,
    [Summary("`Sandbox`, `Production`, `System` or `Demo`")]
    string Tier,
    [Summary("Its database and its workspace, counted as its allowance counts them, in bytes")]
    long DataBytes,
    [Summary("What its tier allows its data, in bytes")]
    long AllowedBytes,
    [Summary("Its versions, as they are kept, in bytes")]
    long VersionBytes,
    [Summary("Its features (drafts), with their copies of the data, in bytes")]
    long DraftBytes,
    [Summary("What is kept for it besides - its git history, its packed source, what it was compiled and unpacked to - in bytes")]
    long OtherBytes,
    [Summary("All of it together, in bytes")]
    long TotalBytes
)
{

    public static AdminLambdaStorage Of(LambdaStorage lambda)
        => new(lambda.PublicKey, LambdaTierNames.Name(lambda.Tier), lambda.DataBytes, lambda.AllowedBytes, lambda.VersionBytes, lambda.DraftBytes, lambda.OtherBytes, lambda.TotalBytes);

}
