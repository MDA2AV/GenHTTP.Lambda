using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// One name in a snippet, and what the compiler says it is.
/// </summary>
/// <remarks>
/// Positions are in the coordinates of the snippet the user wrote, not of the
/// file that gets compiled - the two differ by the scaffolding wrapped around
/// it.
/// </remarks>
public sealed record ClassifiedToken(int Line, int Column, int Length, string Kind);

/// <summary>
/// Asks the compiler what every name in a snippet means, so the editor can
/// colour it for what it is rather than for how it is spelled.
/// </summary>
/// <remarks>
/// The grammar in the browser matches regular expressions: it can tell a
/// keyword from a string and guess at a type from a capital letter, and it is
/// wrong about anything that does not follow the convention. Roslyn is already
/// here to compile these snippets, and it knows the difference between a type,
/// a method, a parameter and a local because it resolved them.
///
/// Nothing is emitted. Binding is what costs, and an assembly nobody will load
/// is not worth writing.
/// </remarks>
public static class SemanticClassifier
{
    private static readonly CSharpCompilationOptions Options =
        new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: false, concurrentBuild: true);

    #region Functionality

    /// <summary>
    /// Classifies every name the compiler can resolve in the given snippet.
    /// </summary>
    public static IReadOnlyList<ClassifiedToken> Classify(string code)
    {
        var snippet = SourceBuilder.ParseSnippet(code);

        var scope = "Lambda_classification";

        var wrapped = SourceBuilder.Wrap(snippet, string.Empty, string.Empty, scope);

        var compilation = CSharpCompilation.Create(scope, [wrapped], ReferenceProvider.Resolve(), Options);

        var model = compilation.GetSemanticModel(wrapped);

        var root = wrapped.GetRoot();

        var tokens = new List<ClassifiedToken>();

        foreach (var node in root.DescendantNodes())
        {
            switch (node)
            {
                // "var" resolves to whatever was inferred, and colouring it
                // as that type contradicts every editor people have used
                case IdentifierNameSyntax { IsVar: true }:
                    break;

                // a name being used
                case SimpleNameSyntax name:
                    Add(tokens, wrapped, name.Identifier, Describe(model.GetSymbolInfo(name).Symbol));
                    break;

                // a name being declared, which no use of it would reach
                case BaseTypeDeclarationSyntax type:
                    Add(tokens, wrapped, type.Identifier, Describe(model.GetDeclaredSymbol(type)));
                    break;

                case MethodDeclarationSyntax method:
                    Add(tokens, wrapped, method.Identifier, "method");
                    break;

                case ParameterSyntax parameter:
                    Add(tokens, wrapped, parameter.Identifier, "parameter");
                    break;

                case VariableDeclaratorSyntax variable:
                    Add(tokens, wrapped, variable.Identifier, Describe(model.GetDeclaredSymbol(variable)));
                    break;
            }
        }

        // the editor wants them in the order it will walk the document
        tokens.Sort((a, b) => a.Line != b.Line ? a.Line - b.Line : a.Column - b.Column);

        return tokens;
    }

    /// <summary>
    /// Records a token, if it belongs to the snippet rather than to the
    /// scaffolding around it.
    /// </summary>
    /// <remarks>
    /// The wrapped file carries line directives pointing back at what the user
    /// wrote, so the mapped span is in their coordinates - and a span that maps
    /// to any other file is generated code they never see.
    /// </remarks>
    private static void Add(List<ClassifiedToken> tokens, SyntaxTree tree, SyntaxToken identifier, string? kind)
    {
        if (kind == null)
        {
            return;
        }

        var mapped = tree.GetMappedLineSpan(identifier.Span);

        if (mapped.Path != SourceBuilder.UserFile)
        {
            return;
        }

        var start = mapped.StartLinePosition;

        tokens.Add(new ClassifiedToken(start.Line, start.Character, identifier.Span.Length, kind));
    }

    /// <summary>
    /// The name the editor knows a symbol by, or null for one it does not
    /// colour differently from ordinary text.
    /// </summary>
    private static string? Describe(ISymbol? symbol) => symbol switch
    {
        INamedTypeSymbol { TypeKind: TypeKind.Interface } => "interface",
        INamedTypeSymbol { TypeKind: TypeKind.Struct } => "struct",
        INamedTypeSymbol { TypeKind: TypeKind.Enum } => "enum",
        INamedTypeSymbol => "type",
        IMethodSymbol => "method",
        IPropertySymbol => "property",
        IFieldSymbol { IsConst: true } => "enumMember",
        IFieldSymbol => "property",
        IParameterSymbol => "parameter",
        ILocalSymbol => "local",
        INamespaceSymbol => "namespace",
        _ => null
    };

    #endregion

}
