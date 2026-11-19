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
    /// What the picture shown with it held while pictures were rows - empty
    /// since they are files of the files integration (<c>showcase/{lambda}</c>),
    /// and left for the platform to move out as it starts.
    /// </summary>
    public byte[] Image { get; set; } = [];

    /// <summary>
    /// How large the picture is, in bytes.
    /// </summary>
    public int ImageBytes { get; set; }

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
