using GenHTTP.Api.Content.Documentation;

using GenHTTP.Lambda.Services.Building;

namespace GenHTTP.Lambda.Api.Model;

/// <summary>
/// What the browser sends: one sentence, and nothing else that identifies
/// anything already made.
/// </summary>
/// <remarks>
/// There is deliberately no way to name an existing lambda here. This endpoint
/// creates, and only creates; changing something that exists belongs to the
/// editor, which already holds its key, or to an agent over MCP. Accepting a
/// key here made one door do two jobs and blurred what the box is for.
/// </remarks>
[Summary("What should be built.")]
public sealed record BuildRequest(
    [Summary("What to build, in the words of whoever wants it, in any language - at least three characters, cut past 2000")]
    string? Prompt,
    [Summary("Which of the offered models builds it, by its name in `build.offer.models` of `GET /system`; left out, the default one (`build.offer.model`)")]
    string? Model = null,
    [Summary("The installation's password, for a model that needs it (`password` of the model)")]
    string? Password = null,
    [Summary("Which of the offered efforts it runs at, as `build.offer.efforts` names them; left out, the default one (`build.offer.effort`)")]
    string? Effort = null
);
