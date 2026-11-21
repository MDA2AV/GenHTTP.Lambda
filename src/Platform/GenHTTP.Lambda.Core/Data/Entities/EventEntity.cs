namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Something that happened, kept after the thing it happened to has gone.
/// </summary>
/// <remarks>
/// Append only. Nothing updates one of these and nothing deletes one, which is
/// what makes them able to answer a question about last week - the tables they
/// sit beside hold the present and overwrite it as it changes.
/// </remarks>
public sealed class EventEntity
{

    public long Id { get; set; }

    /// <summary>What happened.</summary>
    public LambdaEvent Kind { get; set; }

    /// <summary>
    /// Which lambda, where there still is one.
    /// </summary>
    /// <remarks>
    /// Not a foreign key on purpose: a deletion has to survive the row it
    /// refers to, and a cascade would remove exactly the events worth keeping.
    /// </remarks>
    public long? LambdaId { get; set; }

    /// <summary>
    /// The address it was hosted at, kept alongside the id because the id
    /// stops meaning anything once the lambda is gone.
    /// </summary>
    public string? PublicKey { get; set; }

    public DateTime Occurred { get; set; }

}
