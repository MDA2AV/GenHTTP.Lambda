using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace GenHTTP.Lambda.Analyzers;

/// <summary>
/// One top-level type per file, and the file named after it.
/// </summary>
/// <remarks>
/// So a type is found by its name rather than by knowing which file it is
/// hidden in (CLAUDE.md, Working conventions). Every kind of type counts -
/// classes, records, structs, interfaces, enums and delegates. Nested types
/// stay with the type they are nested in, and a <c>file</c> type, which the
/// compiler holds to the file it is declared in, stays with the type that
/// uses it. The parts of a partial type may be named after it with a suffix
/// (<c>McpTools.Lambdas.cs</c>). Generated code is not looked at.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OneTypePerFile : DiagnosticAnalyzer
{

    #region Rules

    public static readonly DiagnosticDescriptor MoreThanOne = new(
        id: "GL0001",
        title: "One type per file",
        messageFormat: "'{0}' is the second top-level type in this file: give it a file of its own, '{0}.cs' (nested types and file types may stay)",
        category: "Layout",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A file declares one top-level type, so a type is found by its name. A group of DTOs becomes a folder.");

    public static readonly DiagnosticDescriptor Misnamed = new(
        id: "GL0002",
        title: "A file is named after its type",
        messageFormat: "The file declares '{0}': name it '{0}.cs'",
        category: "Layout",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A file is named after the one top-level type it declares; the parts of a partial type may add a suffix ('Type.Part.cs').");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(MoreThanOne, Misnamed);

    #endregion

    #region Analysis

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxTreeAction(Analyze);
    }

    private static void Analyze(SyntaxTreeAnalysisContext context)
    {
        if (context.Tree.GetRoot(context.CancellationToken) is not CompilationUnitSyntax unit)
        {
            return;
        }

        var types = TopLevel(unit.Members).Where(t => !t.Modifiers.Any(SyntaxKind.FileKeyword)).ToList();

        foreach (var type in types.Skip(1))
        {
            context.ReportDiagnostic(Diagnostic.Create(MoreThanOne, IdentifierOf(type).GetLocation(), IdentifierOf(type).Text));
        }

        if (types.Count == 1 && !string.IsNullOrEmpty(context.Tree.FilePath))
        {
            var file = Path.GetFileNameWithoutExtension(context.Tree.FilePath);
            var name = IdentifierOf(types[0]).Text;

            if (file != name && !file.StartsWith(name + ".", System.StringComparison.Ordinal))
            {
                context.ReportDiagnostic(Diagnostic.Create(Misnamed, IdentifierOf(types[0]).GetLocation(), name));
            }
        }
    }

    /// <summary>
    /// The types a file declares outside of any other type, in the namespaces
    /// it declares them in.
    /// </summary>
    public static IEnumerable<MemberDeclarationSyntax> TopLevel(SyntaxList<MemberDeclarationSyntax> members)
    {
        foreach (var member in members)
        {
            if (member is BaseNamespaceDeclarationSyntax space)
            {
                foreach (var type in TopLevel(space.Members))
                {
                    yield return type;
                }
            }
            else if (member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax)
            {
                yield return member;
            }
        }
    }

    private static SyntaxToken IdentifierOf(MemberDeclarationSyntax type) => type switch
    {
        BaseTypeDeclarationSyntax declared => declared.Identifier,
        DelegateDeclarationSyntax delegated => delegated.Identifier,
        _ => type.GetFirstToken()
    };

    #endregion

}
