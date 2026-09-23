using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// The examples the installation keeps online, grouped as the editor groups
/// its templates.
/// </summary>
public sealed record ExampleListingResponse(IReadOnlyList<ExampleGroupResponse> Groups);

public sealed record ExampleGroupResponse(string Id, string Name, IReadOnlyList<ExampleSummaryResponse> Examples);

/// <summary>
/// An example as the menu needs it.
/// </summary>
/// <remarks>
/// Without the code, which is the whole of the example and most of its weight.
/// A menu that listed eight of them would carry some forty kilobytes to show
/// eight names, and nothing on the way in has asked to read any of it yet.
/// </remarks>
public sealed record ExampleSummaryResponse(
    string Id,
    string Name,
    string Description,
    string PublicKey,
    string Path,
    string TryPath,
    bool Socket,
    bool Live
);

/// <summary>
/// One example: where it is answering, and the code that is answering.
/// </summary>
/// <param name="Path">Where it is hosted</param>
/// <param name="TryPath">What is worth calling, which is rarely the root</param>
/// <param name="Socket">Whether it is reached by opening a socket rather than asking for a page</param>
/// <param name="Files">Every file it is made of, the snippet first</param>
/// <param name="Live">Whether it is answering right now - they are prepared after startup</param>
public sealed record ExampleResponse(
    string Id,
    string Name,
    string Description,
    string PublicKey,
    string Path,
    string TryPath,
    bool Socket,
    IReadOnlyList<LambdaFile> Files,
    bool Live
);
