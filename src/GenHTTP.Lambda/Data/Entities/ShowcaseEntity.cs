namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// How the owner of a lambda presents it on the showcase page.
/// </summary>
/// <remarks>
/// At most one per lambda, which is why the lambda is the key. Its existence
/// is the opt in: a lambda without one is not listed.
/// </remarks>
public sealed class ShowcaseEntity
{

    public long LambdaId { get; set; }

    public required string Title { get; set; }

    public required string Description { get; set; }

    /// <summary>
    /// The picture shown with it, as uploaded.
    /// </summary>
    public required byte[] Image { get; set; }

    /// <summary>
    /// What the picture is, as sniffed from its first bytes rather than
    /// taken from whoever uploaded it.
    /// </summary>
    public required string ImageType { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// When any of it last changed, which also names the version of the picture.
    /// </summary>
    public DateTime Updated { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
