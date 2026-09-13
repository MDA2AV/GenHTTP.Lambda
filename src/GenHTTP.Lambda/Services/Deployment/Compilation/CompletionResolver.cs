using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// One thing that could be written at a position, and what it is.
/// </summary>
public sealed record ResolvedCompletion(string Label, string Kind, string Detail, string? Documentation);

/// <summary>
/// Answers what may be written at a point in a snippet, by asking the compiler
/// rather than by listing every name the platform knows.
/// </summary>
/// <remarks>
/// The editor used to offer the same few hundred type names wherever the caret
/// was, which is a dictionary rather than a suggestion: it could not complete a
/// member, because knowing what follows a dot means knowing the type of what
/// precedes it, and that needs the compiler. Roslyn is already here.
/// </remarks>
public static class CompletionResolver
{
    private static readonly CSharpCompilationOptions Options =
        new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false, concurrentBuild: true);

    private static readonly SymbolDisplayFormat Signature = new(
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType,
        parameterOptions: SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
    );

    #region Functionality

    /// <summary>
    /// What could be written at the given place in the given snippet.
    /// </summary>
    /// <param name="code">The snippet as the editor has it</param>
    /// <param name="line">The line the caret is on, counting from zero</param>
    /// <param name="column">The column the caret is at, counting from zero</param>
    public static IReadOnlyList<ResolvedCompletion> Resolve(string code, int line, int column)
    {
        (code, column) = Complete(code, line, column);

        var snippet = SourceBuilder.ParseSnippet(code);

        var scope = "Lambda_completion";

        var wrapped = SourceBuilder.Wrap(snippet, string.Empty, scope);

        var compilation = CSharpCompilation.Create(scope, [wrapped], ReferenceProvider.Resolve(), Options);

        var model = compilation.GetSemanticModel(wrapped);

        var position = Locate(wrapped, line, column);

        if (position == null)
        {
            return [];
        }

        var root = wrapped.GetRoot();

        // the caret sits after the dot, and the token starting there belongs to
        // whatever follows it - so the character behind is asked as well
        var access = Access(root, position.Value) ?? Access(root, position.Value - 1);

        return access != null ? Members(model, access) : InScope(model, position.Value);
    }

    /// <summary>
    /// Puts a name after a trailing dot, so the parser has something to attach
    /// it to.
    /// </summary>
    /// <remarks>
    /// "note." is not a statement and the parser builds no member access for
    /// it - there is nothing on the right of the dot to make one with. Which
    /// would be a footnote, except that half written is the state code is in
    /// whenever anybody wants a suggestion. A placeholder makes the expression
    /// whole; it is parsed, never stored, and never seen.
    /// </remarks>
    private static (string Code, int Column) Complete(string code, int line, int column)
    {
        var text = SourceText.From(code);

        if (line >= text.Lines.Count)
        {
            return (code, column);
        }

        var at = text.Lines[line].Start + Math.Min(column, text.Lines[line].Span.Length);

        if (at <= 0 || at > code.Length || code[at - 1] != '.')
        {
            return (code, column);
        }

        // and if a name is already being typed there, it is its own placeholder
        if (at < code.Length && (char.IsLetterOrDigit(code[at]) || code[at] == '_'))
        {
            return (code, column);
        }

        var rest = code[at..];

        var remainder = rest.Length == 0 ? string.Empty : rest.Split('\n')[0];

        // and a statement with no end swallows whatever follows it - including
        // the declarations the expression needs to have a type at all
        var terminator = remainder.TrimEnd().Length == 0 ? "__caret;" : "__caret";

        return (code[..at] + terminator + rest, column);
    }

    /// <summary>
    /// The member access the given position is part of, if it is part of one.
    /// </summary>
    private static MemberAccessExpressionSyntax? Access(SyntaxNode root, int position)
    {
        if (position < 0 || position > root.FullSpan.End)
        {
            return null;
        }

        return root.FindToken(position).Parent?
                   .AncestorsAndSelf()
                   .OfType<MemberAccessExpressionSyntax>()
                   .FirstOrDefault();
    }

    /// <summary>
    /// Everything reachable on whatever sits to the left of the dot.
    /// </summary>
    private static IReadOnlyList<ResolvedCompletion> Members(SemanticModel model, MemberAccessExpressionSyntax access)
    {
        // what is written to the left decides which half of the type is
        // wanted: a type name asks for its static surface, anything else for
        // the instance one. Asking the type of a type name answers the type
        // itself, so the symbol has to be consulted first.
        var symbol = model.GetSymbolInfo(access.Expression).Symbol;

        var isType = symbol is INamedTypeSymbol && symbol is not IMethodSymbol;

        var owner = isType ? symbol as INamedTypeSymbol : model.GetTypeInfo(access.Expression).Type;

        if (owner == null)
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);

        var found = new List<ResolvedCompletion>();

        for (var type = owner; type != null; type = type.BaseType)
        {
            foreach (var member in type.GetMembers())
            {
                if (member.DeclaredAccessibility != Accessibility.Public || member.IsImplicitlyDeclared)
                {
                    continue;
                }

                if (member is IMethodSymbol { MethodKind: not MethodKind.Ordinary })
                {
                    continue;
                }

                if (member.IsStatic != isType || !seen.Add(member.Name))
                {
                    continue;
                }

                found.Add(Describe(member));
            }
        }

        found.Sort((a, b) => string.CompareOrdinal(a.Label, b.Label));

        return found;
    }

    /// <summary>
    /// Every name that is in scope where the caret is.
    /// </summary>
    private static IReadOnlyList<ResolvedCompletion> InScope(SemanticModel model, int position)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        var found = new List<ResolvedCompletion>();

        foreach (var symbol in model.LookupSymbols(position))
        {
            // the scaffolding is in scope here and is not the user's business
            if (symbol.Name.StartsWith("__", StringComparison.Ordinal) || !seen.Add(symbol.Name))
            {
                continue;
            }

            if (symbol is INamespaceSymbol || CodeGuard.IsBanned(symbol.Name))
            {
                continue;
            }

            found.Add(Describe(symbol));
        }

        found.Sort((a, b) => string.CompareOrdinal(a.Label, b.Label));

        return found;
    }

    /// <summary>
    /// Turns a position in the snippet into one in the file that was compiled.
    /// </summary>
    private static int? Locate(SyntaxTree tree, int line, int column)
    {
        var text = tree.GetText();

        // the generated file carries line directives back to the snippet, so
        // the line the editor means is found by asking each line where it came
        // from rather than by counting
        TextLine? best = null;

        var closest = -1;

        for (var i = 0; i < text.Lines.Count; i++)
        {
            var mapped = tree.GetMappedLineSpan(text.Lines[i].Span);

            // the directive announcing a line maps to it as well, and is not
            // the line - landing on it puts the caret before the code it names
            if (mapped.Path != SourceBuilder.UserFile
                || text.Lines[i].ToString().TrimStart().StartsWith('#'))
            {
                continue;
            }

            var at = mapped.StartLinePosition.Line;

            if (at == line)
            {
                var exact = text.Lines[i].Start + Math.Min(column, text.Lines[i].Span.Length);

                return Math.Min(exact, text.Length);
            }

            // typing on a line past the end of what was parsed is ordinary, so
            // the nearest line above it stands in rather than nothing at all
            if (at < line && at > closest)
            {
                closest = at;
                best = text.Lines[i];
            }
        }

        return best == null ? null : Math.Min(best.Value.End, text.Length);
    }

    private static ResolvedCompletion Describe(ISymbol symbol) => new(
        symbol.Name,
        symbol switch
        {
            INamedTypeSymbol { TypeKind: TypeKind.Interface } => "interface",
            INamedTypeSymbol { TypeKind: TypeKind.Struct } => "struct",
            INamedTypeSymbol { TypeKind: TypeKind.Enum } => "enum",
            INamedTypeSymbol => "class",
            IMethodSymbol => "method",
            IPropertySymbol => "property",
            IFieldSymbol => "field",
            IParameterSymbol => "parameter",
            ILocalSymbol => "variable",
            _ => "text"
        },
        symbol.ToDisplayString(Signature),
        symbol.GetDocumentationCommentXml() is { Length: > 0 } xml ? Summarize(xml) : null
    );

    /// <summary>
    /// Pulls the summary out of a documentation comment, without a parser.
    /// </summary>
    private static string? Summarize(string xml)
    {
        var start = xml.IndexOf("<summary>", StringComparison.Ordinal);

        if (start < 0)
        {
            return null;
        }

        var end = xml.IndexOf("</summary>", start, StringComparison.Ordinal);

        if (end < 0)
        {
            return null;
        }

        var summary = xml[(start + 9)..end];

        return string.Join(' ', summary.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    #endregion

}
