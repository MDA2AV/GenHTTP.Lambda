namespace GenHTTP.Lambda.Data.Entities;

/// <summary>
/// Who wrote what a version or a draft holds, where the platform knows it -
/// beside the door it came through (<see cref="VersionOrigin"/>).
/// </summary>
/// <remarks>
/// The door says how a version arrived; this says who wrote it, which is not
/// the same: the owner puts a draft the build agent wrote online through the
/// editor, and the version is then the agent's work that came through the
/// API. Only the build agent of the installation is known so far - the job
/// it runs hands its calls a ticket the server issued (see
/// <c>IBuildService.TicketHeader</c>). An agent of somebody's own says
/// nothing the platform could trust, so what it writes has no writer.
/// Stored as its number, as the origin is; named by
/// <see cref="VersionWriterNames"/>.
/// </remarks>
public enum VersionWriter
{

    /// <summary>The build agent of this installation, on a job it was given.</summary>
    BuildAgent = 1

}
