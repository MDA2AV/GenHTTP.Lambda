using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <summary>
/// Questions the editor asks the compiler about code that has not been saved.
/// </summary>
/// <remarks>
/// Nothing here is stored and nothing is a resource of its own, so each
/// question is a verb that takes the code with it. They all sit behind the
/// editor key regardless, because they run the compiler: an endpoint that
/// binds arbitrary C# for anyone who asks is a way to spend a server.
/// </remarks>
public sealed class CodeResource(IMetaService meta)
{

    /// <summary>
    /// Compiles the code without storing or deploying it.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/check")]
    public async ValueTask<CompilationResponse> Check(string privateKey, CodeRequest request)
    {
        var outcome = await meta.CheckAsync(privateKey, VersionResource.Serialize(request.Files));

        return new CompilationResponse(outcome.Success, outcome.Diagnostics);
    }

    /// <summary>
    /// What every name in a file means, for the colours in the editor.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/semantics")]
    public async ValueTask<SemanticsResponse> Semantics(string privateKey, CodeRequest request)
    {
        await meta.RequireIdAsync(privateKey);

        var tokens = SemanticClassifier.Classify(Select(request));

        return new SemanticsResponse([.. tokens.Select(t => new SemanticToken(t.Line, t.Column, t.Length, t.Kind))]);
    }

    /// <summary>
    /// What could be written where the caret is.
    /// </summary>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/completions")]
    public async ValueTask<CompletionsResponse> Completions(string privateKey, CodeRequest request)
    {
        await meta.RequireIdAsync(privateKey);

        var found = CompletionResolver.Resolve(Select(request), request.Line, request.Column);

        return new CompletionsResponse([.. found.Select(c => new ResolvedCompletionResponse(c.Label, c.Kind, c.Detail, c.Documentation))]);
    }

    /// <summary>
    /// Where the name under the caret was declared.
    /// </summary>
    /// <remarks>
    /// Only ever answers with a file of this lambda. A name that came from the
    /// framework has a declaration, but not one anybody here can be shown, and
    /// sending the editor to a file that does not exist is worse than telling
    /// it there is nowhere to go.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/definition")]
    public async ValueTask<DefinitionResponse> Definition(string privateKey, CodeRequest request)
    {
        await meta.RequireIdAsync(privateKey);

        var files = request.Files ?? [];

        var found = DefinitionResolver.Resolve(files,
                                               request.File ?? files.FirstOrDefault()?.Name ?? LambdaSource.EntryName,
                                               request.Line,
                                               request.Column);

        return found == null
             ? new DefinitionResponse(null, 0, 0, 0)
             : new DefinitionResponse(found.File, found.Line, found.Column, found.Length);
    }

    /// <summary>
    /// The code of the file a single file question is about.
    /// </summary>
    private static string Select(CodeRequest request)
    {
        var files = request.Files ?? [];

        var file = request.File == null
                 ? files.FirstOrDefault()
                 : files.FirstOrDefault(f => f.Name == request.File)
                   ?? throw LambdaException.Invalid($"There is no file called '{request.File}' in the request.");

        return file?.Code ?? string.Empty;
    }

}
