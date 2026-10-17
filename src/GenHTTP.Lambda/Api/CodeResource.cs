using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Api.Infrastructure;
using GenHTTP.Lambda.Api.Model;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Deployment.Compilation;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Meta;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace GenHTTP.Lambda.Api;

/// <remarks>
/// Nothing here is stored and nothing is a resource of its own, so each
/// question is a verb that takes the code with it. They all sit behind the
/// editor key regardless, because they run the compiler: an endpoint that
/// binds arbitrary C# for anyone who asks is a way to spend a server.
/// </remarks>
[Tag(ApiDocumentation.Code)]
[Summary("Questions for the compiler about code that has not been saved: does it compile, and what does a name mean.")]
[Remarks("""
         Each takes the files with it, as a save would, and stores nothing. They are behind the editor key of a lambda because
         they run the compiler, against the references and the rules a lambda is compiled with - so code that passes the
         check compiles when it is deployed. Positions count lines and columns from zero.
         """)]
[Response(ResponseStatus.NotFound, ApiDocumentation.NoLambda, Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
public sealed class CodeResource(IMetaService meta)
{

    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/check")]
    [Summary("Compiles code without saving it")]
    [Remarks("""
             Exactly as a deployment would compile them, including the checks of what a lambda may not do, but without running
             anything. Code that does not compile is not an error of the request: it is answered with `200` and
             `success: false`. The files are held to the same rules as a save.
             """)]
    [Response(ResponseStatus.BadRequest, "The files cannot be a lambda as they are - `lambda.cs` missing or not first, a name that cannot be used - or they are larger than a version may be.",
              Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Whether it compiles, and what the compiler said")]
    public async ValueTask<CompilationResponse> Check([Summary(ApiDocumentation.PrivateKey)] string privateKey, CodeRequest request)
    {
        var outcome = await meta.CheckAsync(privateKey, VersionResource.Serialize(request.Files));

        return new CompilationResponse(outcome.Success, outcome.Diagnostics);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/semantics")]
    [Summary("Classifies the names in a file")]
    [Remarks("For the file named by `file`, the first one if none is: where each name is, and what the compiler resolved it to.")]
    [Response(ResponseStatus.BadRequest, "There is no file of that name in the request.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("Every name of the file, and what it means")]
    public async ValueTask<SemanticsResponse> Semantics([Summary(ApiDocumentation.PrivateKey)] string privateKey, CodeRequest request)
    {
        meta.RequireId(privateKey);

        // binding with Roslyn, asked while somebody types: tens to hundreds of
        // milliseconds that a reactor would not be serving anybody (see Offload)
        var tokens = await Offload.Run(() => SemanticClassifier.Classify(Select(request)));

        return new SemanticsResponse([.. tokens.Select(t => new SemanticToken(t.Line, t.Column, t.Length, t.Kind))]);
    }

    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/completions")]
    [Summary("Suggests what could be written")]
    [Remarks("At `line` and `column` of the file named by `file`: the members, types and keywords the compiler would accept there.")]
    [Response(ResponseStatus.BadRequest, "There is no file of that name in the request.", Type = typeof(ErrorResponse), ContentType = ApiDocumentation.Json)]
    [return: Summary("What could be written there")]
    public async ValueTask<CompletionsResponse> Completions([Summary(ApiDocumentation.PrivateKey)] string privateKey, CodeRequest request)
    {
        meta.RequireId(privateKey);

        var found = await Offload.Run(() => CompletionResolver.Resolve(Select(request), request.Line, request.Column));

        return new CompletionsResponse([.. found.Select(c => new ResolvedCompletionResponse(c.Label, c.Kind, c.Detail, c.Documentation))]);
    }

    /// <remarks>
    /// Only ever answers with a file of this lambda. A name that came from the
    /// framework has a declaration, but not one anybody here can be shown, and
    /// sending the editor to a file that does not exist is worse than telling
    /// it there is nowhere to go.
    /// </remarks>
    [ResourceMethod(Method.Post, "lambdas/:privateKey/code/definition")]
    [Summary("Finds where a name was declared")]
    [Remarks("Only ever in a file of the request. A name declared elsewhere - by the framework, say - is answered with no file.")]
    [return: Summary("Where the name was declared, or no file where that is in none of them")]
    public async ValueTask<DefinitionResponse> Definition([Summary(ApiDocumentation.PrivateKey)] string privateKey, CodeRequest request)
    {
        meta.RequireId(privateKey);

        var files = request.Files ?? [];

        var found = await Offload.Run(() => DefinitionResolver.Resolve(files,
                                                                       request.File ?? files.FirstOrDefault()?.Name ?? LambdaSource.EntryName,
                                                                       request.Line,
                                                                       request.Column));

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
