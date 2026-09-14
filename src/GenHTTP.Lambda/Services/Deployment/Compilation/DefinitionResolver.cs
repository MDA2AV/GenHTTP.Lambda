using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Where a name was declared, so the editor can go there.
/// </summary>
/// <param name="File">The file of the user it was declared in</param>
/// <param name="Line">The line it is on, counting from zero</param>
/// <param name="Column">The column it starts at, counting from zero</param>
/// <param name="Length">How long the name itself is</param>
public sealed record ResolvedDefinition(string File, int Line, int Column, int Length);

/// <summary>
/// Finds the declaration of whatever a name refers to.
/// </summary>
/// <remarks>
/// This works because of something already there for another reason. The file
/// handed to the compiler is generated, and it carries <c>#line</c> directives
/// pointing back at the file the user wrote - which exist so that a diagnostic
/// names the user's line rather than a line of scaffolding. Roslyn honours
/// them, so asking any position in the generated tree where it came from gives
/// an answer in the user's own files, and the whole of the mapping problem is
/// already solved.
///
/// Unlike completions, this compiles every file rather than only the snippet:
/// the point of it is to cross from one file to another.
/// </remarks>
public static class DefinitionResolver
{
    private static readonly CSharpCompilationOptions Options =
        new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false, concurrentBuild: true);

    #region Functionality

    /// <summary>
    /// Where the name under the caret was declared, if it was declared here.
    /// </summary>
    /// <param name="files">Every file of the lambda, the snippet first</param>
    /// <param name="file">The file the caret is in</param>
    /// <param name="line">The line it is on, counting from zero</param>
    /// <param name="column">The column it is at, counting from zero</param>
    /// <returns>Where to go, or null if there is nowhere to go</returns>
    public static ResolvedDefinition? Resolve(IReadOnlyList<LambdaFile> files, string file, int line, int column)
    {
        if (files.Count == 0)
        {
            return null;
        }

        var scope = "Lambda_definition";

        var trees = new List<SyntaxTree>();

        foreach (var one in files)
        {
            if (!one.IsCode)
            {
                continue;
            }

            trees.Add(one.Name == LambdaSource.EntryName
                    ? SourceBuilder.Wrap(SourceBuilder.ParseSnippet(one.Code), string.Empty, string.Empty, scope)
                    : SourceBuilder.WrapFile(SourceBuilder.ParseFile(one.Code, one.Name), scope, one.Name));
        }

        if (trees.Count == 0)
        {
            return null;
        }

        var compilation = CSharpCompilation.Create(scope, trees, ReferenceProvider.Resolve(), Options);

        foreach (var tree in trees)
        {
            var at = Locate(tree, file, line, column);

            if (at == null)
            {
                continue;
            }

            var found = Declaration(compilation, tree, at.Value);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// Asks what the token at a position refers to, and where that was written.
    /// </summary>
    private static ResolvedDefinition? Declaration(CSharpCompilation compilation, SyntaxTree tree, int position)
    {
        var root = tree.GetRoot();

        var token = root.FindToken(position);

        if (!token.IsKind(SyntaxKind.IdentifierToken))
        {
            return null;
        }

        var model = compilation.GetSemanticModel(tree);

        var node = token.Parent;

        if (node == null)
        {
            return null;
        }

        /*
         * A name is asked about twice. GetSymbolInfo answers for a use of
         * something, which is the common case; GetDeclaredSymbol answers when
         * the caret is on the declaration itself, where there is nothing to
         * look up because this is the thing being looked up. Offering both
         * means control-clicking a method's own name jumps to nothing rather
         * than to somewhere surprising.
         */
        var symbol = model.GetSymbolInfo(node).Symbol
                  ?? model.GetSymbolInfo(node).CandidateSymbols.FirstOrDefault()
                  ?? model.GetDeclaredSymbol(node);

        if (symbol == null)
        {
            return null;
        }

        // a local or a parameter declares itself where it is written; a type or
        // a method may have been written in another file of the same lambda
        foreach (var reference in symbol.DeclaringSyntaxReferences)
        {
            var declared = reference.GetSyntax();

            var name = Named(declared);

            var mapped = declared.SyntaxTree.GetMappedLineSpan(name);

            if (IsUsers(mapped.Path))
            {
                return new ResolvedDefinition(mapped.Path,
                                              mapped.StartLinePosition.Line,
                                              mapped.StartLinePosition.Character,
                                              Math.Max(1, name.Length));
            }
        }

        return null;
    }

    /// <summary>
    /// The span of the name of a declaration rather than the whole of it.
    /// </summary>
    /// <remarks>
    /// Jumping to a class puts the caret on its name, not on the line above
    /// where its documentation starts.
    /// </remarks>
    private static TextSpan Named(SyntaxNode declared)
        => declared switch
        {
            // a record is a BaseTypeDeclaration too, so this arm covers it
            Microsoft.CodeAnalysis.CSharp.Syntax.BaseTypeDeclarationSyntax type => type.Identifier.Span,
            Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax method => method.Identifier.Span,
            Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax property => property.Identifier.Span,
            Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax variable => variable.Identifier.Span,
            Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax parameter => parameter.Identifier.Span,
            Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax local => local.Identifier.Span,
            _ => declared.Span
        };

    /// <summary>
    /// Whether a mapped path names a file the user can be sent to.
    /// </summary>
    /// <remarks>
    /// Anything generated, and anything in the framework, maps to a path that
    /// is not one of theirs. There is nowhere to go for those, and saying so
    /// is better than opening a file that does not exist.
    /// </remarks>
    private static bool IsUsers(string path)
        => !string.IsNullOrEmpty(path)
        && !path.EndsWith(".generated.cs", StringComparison.Ordinal)
        && path != SourceBuilder.GeneratedFile;

    /// <summary>
    /// The position in a generated tree that a place in a user's file became.
    /// </summary>
    /// <remarks>
    /// The same trick the completions use, widened to any file rather than
    /// only the snippet: every line of the generated tree is asked where it
    /// came from until one of them says it came from here.
    /// </remarks>
    private static int? Locate(SyntaxTree tree, string file, int line, int column)
    {
        var text = tree.GetText();

        foreach (var candidate in text.Lines)
        {
            var mapped = tree.GetMappedLineSpan(candidate.Span);

            if (mapped.Path != file || mapped.StartLinePosition.Line != line)
            {
                continue;
            }

            // the directive announcing a line maps to it as well and is not it
            if (candidate.ToString().TrimStart().StartsWith('#'))
            {
                continue;
            }

            var at = candidate.Start + column;

            return at <= candidate.End ? at : candidate.End;
        }

        return null;
    }

    #endregion

}
