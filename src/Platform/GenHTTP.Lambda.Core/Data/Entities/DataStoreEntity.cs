namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// That the owner of a lambda switched one kind of data on or off.
/// </summary>
/// <remarks>
/// Only a choice is stored. A lambda without a row for a kind has what that
/// kind has by default (see <c>DataKinds</c>), so the workspace every lambda
/// always had needs no row, and a kind added later needs none either.
/// </remarks>
public sealed class DataStoreEntity
{

    public long LambdaId { get; set; }

    /// <summary>
    /// Which kind of data, one of <c>DataKinds</c>.
    /// </summary>
    public required string Kind { get; set; }

    public bool Enabled { get; set; }

    /// <summary>
    /// When it was last switched.
    /// </summary>
    public DateTime Changed { get; set; }

    public LambdaEntity? Lambda { get; set; }

}
