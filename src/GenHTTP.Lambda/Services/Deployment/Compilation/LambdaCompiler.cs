using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Compiles a snippet into an assembly and pulls the handler out of it.
/// </summary>
/// <remarks>
/// The assembly is written to disk and loaded into the default context on
/// purpose: GenHTTP compiles invocation code for the handlers it is given at
/// runtime, and that generated code can only reference assemblies that are
/// loadable by name and have a file behind them. A collectible context would
/// be unloadable but invisible to it, so lambdas stay loaded for the lifetime
/// of the process - identical code is recognized and reused rather than
/// compiled twice.
/// </remarks>
internal static class LambdaCompiler
{
    private static readonly ConcurrentDictionary<string, LoadedLambda> Loaded = [];

    private static readonly CSharpCompilationOptions CompilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Release,
            allowUnsafe: false,
            concurrentBuild: false,
            deterministic: false,
            nullableContextOptions: NullableContextOptions.Disable
        )
        .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
        {
            // "async method lacks await" - most snippets are plain synchronous code
            ["CS1998"] = ReportDiagnostic.Suppress
        });

    #region Functionality

    /// <summary>
    /// Compiles the snippet and, unless <paramref name="run" /> is disabled,
    /// loads and invokes it to obtain the handler.
    /// </summary>
    /// <param name="request">What to compile and where to put it</param>
    internal static async ValueTask<(CompilationOutcome Outcome, IHandler? Handler)> CompileAsync(CompilationRequest request)
    {
        var snippet = SourceBuilder.ParseSnippet(request.Files[0].Code);

        // the snippet is script, the rest are ordinary C# holding types
        var others = request.Files.Skip(1)
                            .Select(f => (File: f, Tree: SourceBuilder.ParseFile(f.Code, f.Name)))
                            .ToList();

        var syntaxErrors = Translate(snippet.GetDiagnostics());

        foreach (var other in others)
        {
            syntaxErrors.AddRange(Translate(other.Tree.GetDiagnostics()));
        }

        if (syntaxErrors.Count > 0)
        {
            return (CompilationOutcome.Failed(syntaxErrors), null);
        }

        var rejections = new List<CompilationDiagnostic>(CodeGuard.Inspect(await snippet.GetRootAsync()));

        // every file is the user's, so every file is inspected - a guard that
        // only looked at the snippet would be avoided by moving the code
        foreach (var other in others)
        {
            rejections.AddRange(CodeGuard.Inspect(await other.Tree.GetRootAsync()));
        }

        if (rejections.Count > 0)
        {
            return (CompilationOutcome.Failed(rejections), null);
        }

        if (request.Run && Loaded.TryGetValue(Identify(request), out var known))
        {
            // the very same code is already in the process, just build it again
            return (CompilationOutcome.Succeeded(), await InvokeAsync(known));
        }

        var scope = $"Lambda_{request.Name}_{Guid.NewGuid():N}";

        var trees = new List<SyntaxTree> { SourceBuilder.Wrap(snippet, request.Workspace, scope) };

        foreach (var other in others)
        {
            trees.Add(SourceBuilder.WrapFile(other.Tree, scope, other.File.Name));
        }

        var compilation = CSharpCompilation.Create(
            scope,
            trees,
            ReferenceProvider.Resolve(),
            CompilationOptions
        );

        if (!request.Run)
        {
            using var check = new MemoryStream();

            var probe = compilation.Emit(check);

            return (probe.Success ? CompilationOutcome.Succeeded(Translate(probe.Diagnostics, DiagnosticSeverity.Warning))
                                  : CompilationOutcome.Failed(Translate(probe.Diagnostics)), null);
        }

        Directory.CreateDirectory(request.AssemblyDirectory);

        var file = Path.Combine(request.AssemblyDirectory, $"{scope}.dll");

        var emitted = compilation.Emit(file);

        if (!emitted.Success)
        {
            TryDelete(file);

            return (CompilationOutcome.Failed(Translate(emitted.Diagnostics)), null);
        }

        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);

        var lambda = new LoadedLambda(assembly, scope);

        Loaded[Identify(request)] = lambda;

        return (CompilationOutcome.Succeeded(Translate(emitted.Diagnostics, DiagnosticSeverity.Warning)), await InvokeAsync(lambda));
    }

    /// <summary>
    /// Runs the entry point of a compiled lambda and normalizes whatever it
    /// returned. Called on every deployment, so redeploying resets the state a
    /// snippet holds in memory.
    /// </summary>
    private static async ValueTask<IHandler> InvokeAsync(LoadedLambda lambda)
    {
        var entry = lambda.Assembly.GetType($"{lambda.Scope}.{SourceBuilder.EntryType}")
                 ?? throw new InvalidOperationException("The compiled lambda does not contain an entry point.");

        var method = entry.GetMethod(SourceBuilder.EntryMethod, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                  ?? throw new InvalidOperationException("The compiled lambda does not contain an entry point.");

        var invocation = (Task<object>?)method.Invoke(null, null);

        var result = invocation == null ? null : await invocation;

        return result switch
        {
            IHandler handler => handler,
            IHandlerBuilder builder => builder.Build(),
            null => throw new InvalidOperationException("The lambda returned null instead of a handler."),
            _ => throw new InvalidOperationException($"The lambda returned '{result.GetType().Name}', which is neither an IHandler nor an IHandlerBuilder.")
        };
    }

    /// <summary>
    /// Identifies a snippet by what it compiles to, so redeploying unchanged
    /// code does not add another assembly to the process.
    /// </summary>
    private static string Identify(CompilationRequest request)
    {
        var builder = new StringBuilder(request.Workspace);

        foreach (var file in request.Files)
        {
            builder.Append('\n').Append(file.Name).Append('\n').Append(file.Code);
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));

        return Convert.ToHexStringLower(bytes);
    }

    private static void TryDelete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (Exception)
        {
            // a leftover artifact is cleaned up on the next start
        }
    }

    /// <summary>
    /// Maps compiler messages back onto the snippet the user wrote.
    /// </summary>
    private static List<CompilationDiagnostic> Translate(IEnumerable<Diagnostic> diagnostics, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        var result = new List<CompilationDiagnostic>();

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic.Severity != severity)
            {
                continue;
            }

            var span = diagnostic.Location.GetMappedLineSpan();

            // a mapped path is one of the user's files; anything else came out
            // of the generated wrapper and has no line worth pointing at
            var written = span.Path != SourceBuilder.GeneratedFile
                       && !span.Path.EndsWith(".generated.cs", StringComparison.Ordinal);

            result.Add(new CompilationDiagnostic(
                severity == DiagnosticSeverity.Error ? "Error" : "Warning",
                diagnostic.Id,
                Describe(diagnostic),
                written ? span.StartLinePosition.Line + 1 : 0,
                written ? span.StartLinePosition.Character + 1 : 0,
                written ? (span.Path.Length > 0 ? span.Path : SourceBuilder.UserFile) : null
            ));
        }

        return result;
    }

    private static string Describe(Diagnostic diagnostic) => diagnostic.Id switch
    {
        // raised on the generated entry point, so the raw message would point nowhere
        "CS0161" => "The code must end with a return statement that returns a handler.",
        _ => diagnostic.GetMessage()
    };

    private sealed record LoadedLambda(Assembly Assembly, string Scope);

    #endregion

}

/// <summary>
/// What the compiler needs to build a snippet.
/// </summary>
/// <param name="Code">The snippet as written by the user</param>
/// <param name="Workspace">The directory the lambda may read and write</param>
/// <param name="AssemblyDirectory">Where the generated assembly is written to</param>
/// <param name="Name">A readable prefix for the generated namespace and assembly</param>
/// <param name="Run">Whether the result should be loaded and invoked, or only checked</param>
internal sealed record CompilationRequest(IReadOnlyList<LambdaFile> Files, string Workspace, string AssemblyDirectory, string Name, bool Run);
