using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Data.Entities;

namespace GenHTTP.Lambda.Services.Census;

/// <summary>
/// The room the installation has and what takes it.
/// </summary>
/// <param name="Directory">The data directory, where everything the platform keeps is</param>
/// <param name="DiskBytes">How large the volume it is on is, where the system says</param>
/// <param name="FreeBytes">How much of that is free for the platform, read as it is asked for</param>
/// <param name="Measured">When what follows was measured; a walk over every file, so it is not measured on every request</param>
/// <param name="Parts">What the data directory holds, by part, largest first</param>
/// <param name="Lambdas">How many lambdas the measurement found something of</param>
/// <param name="Largest">The lambdas that take the most room, largest first</param>
/// <param name="Over">The lambdas whose data is beyond what their tier allows - kept, never written to again until it shrinks</param>
public sealed record StorageCensus(
    string Directory,
    long? DiskBytes,
    long? FreeBytes,
    DateTime? Measured,
    IReadOnlyList<StoragePart> Parts,
    int Lambdas,
    IReadOnlyList<LambdaStorage> Largest,
    IReadOnlyList<LambdaStorage> Over
);
