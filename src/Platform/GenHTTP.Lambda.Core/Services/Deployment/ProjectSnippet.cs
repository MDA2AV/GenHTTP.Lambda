using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using GenHTTP.Lambda.Services.Deployment.Compilation;

namespace GenHTTP.Lambda.Services.Deployment;

/// <summary>
/// The snippet of a lambda as the class <c>Project</c> of a .NET project, and
/// that class as the snippet again.
/// </summary>
/// <remarks>
/// A snippet ends in a return, which at the top of a file would end the
/// program rather than produce a handler, and it may end with a record or two,
/// which cannot be declared inside a method - so its statements become the
/// body of a method of <c>Project</c> and its types sit beside the class.
/// Leading comments travel with what they stand in front of.
///
/// The way back is what a push to the lambda's repository takes: whoever
/// changed <c>Project.cs</c> changed the snippet. It takes the method body
/// out again, moved back to where it stood, with the imports above it and
/// the types below - so a snippet put into the class and taken out once more
/// is what it was, and a class taken out and put back is the class it was.
/// </remarks>
public static class ProjectSnippet
{

    /// <summary>
    /// The name of the class the snippet becomes.
    /// </summary>
    public const string Class = "Project";

    /// <summary>
    /// How far the statements are moved in to sit in the method.
    /// </summary>
    private const int Indent = 8;

    /// <summary>
    /// The members of <see cref="Class"/> that are the platform's rather than the snippet's.
    /// </summary>
    private static readonly HashSet<string> Frame = ["Assets", "Create", "CreateAsync"];

    #region Wrapping

    /// <summary>
    /// The snippet as the class of an exported project: <c>Create()</c>, or
    /// <c>CreateAsync()</c> where its statements wait for something.
    /// </summary>
    public static string ForExport(string snippet)
    {
        var tree = SourceBuilder.ParseSnippet(snippet);

        return Wrap(tree, Awaits(tree));
    }

    /// <summary>
    /// The snippet as the class of the project a lambda's repository holds:
    /// always <c>CreateAsync()</c>, as the platform runs it.
    /// </summary>
    /// <remarks>
    /// Always the same shape, whatever the statements do, because the rest of
    /// the project - Program.cs above all - is the platform's and the same
    /// for every commit: a push that starts awaiting something must not have
    /// to change it. A method that waits for nothing is merely warned about,
    /// and the project does not warn about that.
    /// </remarks>
    public static string ForRepository(string snippet) => Wrap(SourceBuilder.ParseSnippet(snippet), true);

    /// <summary>
    /// Whether the statements of the snippet wait for something, which
    /// decides whether the method they become has to be asynchronous.
    /// </summary>
    /// <remarks>
    /// Most snippets do not, and a synchronous Project.Create() is the
    /// simpler thing to read. Waiting inside a lambda or a local function
    /// does not count, because that one is asynchronous on its own.
    /// </remarks>
    public static bool Awaits(string snippet) => Awaits(SourceBuilder.ParseSnippet(snippet));

    private static bool Awaits(SyntaxTree snippet)
    {
        var root = (CompilationUnitSyntax)snippet.GetRoot();

        return root.Members.OfType<GlobalStatementSyntax>()
                   .SelectMany(s => s.DescendantNodesAndSelf(n => n is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax)))
                   .Any(n => n switch
                   {
                       AwaitExpressionSyntax => true,
                       CommonForEachStatementSyntax loop => loop.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword),
                       UsingStatementSyntax block => block.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword),
                       LocalDeclarationStatementSyntax declaration => declaration.AwaitKeyword.IsKind(SyntaxKind.AwaitKeyword),
                       _ => false
                   });
    }

    private static string Wrap(SyntaxTree snippet, bool asynchronous)
    {
        var root = (CompilationUnitSyntax)snippet.GetRoot();

        var text = snippet.GetText();

        var builder = new StringBuilder();

        foreach (var import in root.Usings)
        {
            builder.Append(import.NormalizeWhitespace().ToFullString()).Append('\n');
        }

        if (root.Usings.Count > 0)
        {
            builder.Append('\n');
        }

        builder.Append($"public static class {Class}").Append('\n');
        builder.Append("{").Append('\n');
        builder.Append("    // Workspace, Resources, Secret and Database come from Platform/, for every file").Append('\n');

        // what the resources were called before, which a snippet written then
        // still says - and in a class, Assets is the Files module's type
        if (SaysAssets(root))
        {
            builder.Append("    private static Platform.ResourceFolder Assets => Platform.LambdaEnvironment.Resources;").Append('\n');
        }

        builder.Append('\n');

        if (asynchronous)
        {
            builder.Append("    public static async Task<IHandler> CreateAsync() => Platform.Handlers.From(await BuildAsync());").Append('\n');
            builder.Append('\n');
            builder.Append("    private static async Task<object> BuildAsync()").Append('\n');
        }
        else
        {
            builder.Append("    public static IHandler Create() => Platform.Handlers.From(Build());").Append('\n');
            builder.Append('\n');
            builder.Append("    private static object Build()").Append('\n');
        }

        builder.Append("    {").Append('\n');

        foreach (var line in Body(root, text))
        {
            builder.Append(line).Append('\n');
        }

        builder.Append("    }").Append('\n');
        builder.Append("}").Append('\n');

        foreach (var member in root.Members.Where(IsType))
        {
            builder.Append('\n');
            builder.Append(Public(member, text).Trim('\r', '\n')).Append('\n');
        }

        return builder.ToString();
    }

    private static bool IsType(MemberDeclarationSyntax member) => member is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax;

    /// <summary>
    /// Whether the statements of the snippet name Assets, which is what the
    /// resources were called before.
    /// </summary>
    private static bool SaysAssets(CompilationUnitSyntax root)
        => root.Members.Where(m => !IsType(m))
               .SelectMany(m => m.DescendantTokens())
               .Any(t => t.IsKind(SyntaxKind.IdentifierToken) && t.ValueText == "Assets");

    /// <summary>
    /// The lines of the statements, moved eight spaces in to sit in the method.
    /// </summary>
    /// <remarks>
    /// A line that starts inside a token - a verbatim or raw string spanning
    /// lines - is left as it is, because its whitespace is part of the string.
    /// </remarks>
    private static List<string> Body(CompilationUnitSyntax root, SourceText text)
    {
        var numbers = new SortedSet<int>();

        foreach (var member in root.Members.Where(m => !IsType(m)))
        {
            var first = text.Lines.GetLineFromPosition(member.FullSpan.Start).LineNumber;
            var last = text.Lines.GetLineFromPosition(Math.Max(member.FullSpan.Start, member.FullSpan.End - 1)).LineNumber;

            for (var number = first; number <= last; number++)
            {
                numbers.Add(number);
            }
        }

        var lines = new List<string>();

        foreach (var number in numbers)
        {
            var line = text.Lines[number];

            var content = line.ToString();

            lines.Add(Inside(root, line.Start) ? content : content.Trim().Length == 0 ? string.Empty : new string(' ', Indent) + content);
        }

        // the blank lines that stood between the usings, the statements and the types
        while (lines.Count > 0 && lines[0].Length == 0)
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }

    /// <summary>
    /// A type declared in the snippet, made public on the way.
    /// </summary>
    /// <remarks>
    /// As the platform does: GenHTTP generates the code that invokes a handler
    /// into an assembly of its own, so every type in the signature of a
    /// handler has to be visible from outside this one.
    /// </remarks>
    private static string Public(MemberDeclarationSyntax member, SourceText text)
    {
        var original = text.ToString(member.FullSpan);

        var modifiers = member.Modifiers.Where(IsAccessibility).ToList();

        if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
        {
            return original;
        }

        var start = member.FullSpan.Start;

        var at = member.AttributeLists.Count > 0
               ? member.AttributeLists.Last().GetLastToken().GetNextToken().SpanStart
               : member.GetFirstToken().SpanStart;

        // from the back, so every position stays where it was; at the same
        // position the modifier goes before "public" comes in
        var edits = modifiers.Select(m => (Start: m.SpanStart, Length: m.FullSpan.End - m.SpanStart, Text: string.Empty))
                             .Append((Start: at, Length: 0, Text: "public "))
                             .OrderByDescending(e => e.Start)
                             .ThenByDescending(e => e.Length);

        var builder = new StringBuilder(original);

        foreach (var (position, length, insert) in edits)
        {
            builder.Remove(position - start, length).Insert(position - start, insert);
        }

        return builder.ToString();
    }

    private static bool IsAccessibility(SyntaxToken token) => token.Kind() is SyntaxKind.PublicKeyword
        or SyntaxKind.InternalKeyword or SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.FileKeyword;

    #endregion

    #region Unwrapping

    /// <summary>
    /// The snippet a class <c>Project</c> holds: the imports above it, the
    /// body of its <c>BuildAsync()</c> - or <c>Build()</c>, as an export has
    /// it - moved back out, and the types beside the class below.
    /// </summary>
    /// <remarks>
    /// The body is moved out by as much as it was moved in - eight spaces, or
    /// less where all of it stands less far in - so the lines keep where they
    /// stand to each other. A line that starts inside a string is left alone.
    /// What the class holds besides the method - the resources by their old
    /// name, the method that
    /// makes the handler - is the platform's and is left behind; anything
    /// else in it has no place in a snippet, and is refused.
    /// </remarks>
    /// <returns>The snippet and where each of its lines came from, or why there is none</returns>
    public static SnippetUnwrapped Unwrap(string project)
    {
        var tree = CSharpSyntaxTree.ParseText(project, SourceBuilder.RegularOptions, "Project.cs");

        var root = (CompilationUnitSyntax)tree.GetRoot();

        var text = tree.GetText();

        var classes = root.Members.OfType<ClassDeclarationSyntax>().Where(c => c.Identifier.Text == Class).ToList();

        if (classes.Count != 1)
        {
            return SnippetUnwrapped.Refused($"Project.cs keeps the class {Class}: its BuildAsync() holds the code of the lambda, what lambda.cs is on the platform.");
        }

        var frame = classes[0];

        var methods = frame.Members.OfType<MethodDeclarationSyntax>().Where(m => m.Identifier.Text is "BuildAsync" or "Build").ToList();

        if (methods.Count != 1 || methods[0].Body is not { } body)
        {
            return SnippetUnwrapped.Refused($"The class {Class} in Project.cs keeps one method BuildAsync() with a body: that body is the code of the lambda.");
        }

        foreach (var member in frame.Members)
        {
            if (member == methods[0] || Frame.Contains(NameOf(member)))
            {
                continue;
            }

            return SnippetUnwrapped.Refused($"'{NameOf(member)}' cannot stay in the class {Class}: it only holds BuildAsync(), whose body is the code of the lambda. "
                                          + "Declare it in BuildAsync() as a local, or in a type of its own below the class or in a file of its own.");
        }

        if (root.AttributeLists.Count > 0 || root.Externs.Count > 0)
        {
            return SnippetUnwrapped.Refused("Project.cs cannot hold assembly attributes or extern aliases: the lambda has no place for them.");
        }

        var lines = new List<string>();

        var origins = new List<(int Line, int Removed)>();

        if (root.Usings.Count > 0)
        {
            // with what is written above them - the comment a snippet opens with
            var first = text.Lines.GetLineFromPosition(root.Usings[0].FullSpan.Start).LineNumber;
            var last = text.Lines.GetLineFromPosition(root.Usings[^1].Span.End).LineNumber;

            for (var number = first; number <= last; number++)
            {
                var line = text.Lines[number].ToString().TrimEnd();

                if (lines.Count > 0 || line.Length > 0)
                {
                    lines.Add(line);
                    origins.Add((number, 0));
                }
            }

            lines.Add(string.Empty);
            origins.Add((-1, 0));
        }

        var statements = Statements(root, text, body);

        if (statements.Count > 0)
        {
            lines.AddRange(statements.Select(s => s.Text));
            origins.AddRange(statements.Select(s => (s.Line, s.Removed)));
        }

        foreach (var member in root.Members.Where(m => m != frame))
        {
            if (member is not (BaseTypeDeclarationSyntax or DelegateDeclarationSyntax))
            {
                return SnippetUnwrapped.Refused("Beside the class Project, Project.cs holds types and nothing else: the code of the lambda goes into BuildAsync().");
            }

            var declared = text.ToString(member.FullSpan).Trim('\r', '\n');

            // where the declaration starts, once the blank lines before it are gone
            var start = member.FullSpan.Start + text.ToString(member.FullSpan).IndexOf(declared, StringComparison.Ordinal);

            var first = text.Lines.GetLineFromPosition(start).LineNumber;

            if (lines.Count > 0)
            {
                lines.Add(string.Empty);
                origins.Add((-1, 0));
            }

            var parts = declared.Split('\n');

            for (var i = 0; i < parts.Length; i++)
            {
                lines.Add(parts[i].TrimEnd('\r'));
                origins.Add((first + i, 0));
            }
        }

        // a line break is a line feed, whatever Project.cs was written with -
        // which is what putting it back into the class writes, too
        return new SnippetUnwrapped(string.Join('\n', lines) + "\n", null, origins);
    }

    /// <summary>
    /// The lines of the method's body, moved back out.
    /// </summary>
    private static List<(string Text, int Line, int Removed)> Statements(CompilationUnitSyntax root, SourceText text, BlockSyntax body)
    {
        var open = text.Lines.GetLineFromPosition(body.OpenBraceToken.SpanStart).LineNumber;
        var close = text.Lines.GetLineFromPosition(body.CloseBraceToken.SpanStart).LineNumber;

        var lines = new List<(string Text, int Line, bool Inside)>();

        if (open == close)
        {
            // all on one line: what stands between the braces
            var between = text.ToString(TextSpan.FromBounds(body.OpenBraceToken.Span.End, body.CloseBraceToken.SpanStart)).Trim();

            if (between.Length > 0)
            {
                lines.Add((between, open, false));
            }
        }
        else
        {
            // what follows the opening brace on its line, and what precedes the closing one on its
            var after = text.ToString(TextSpan.FromBounds(body.OpenBraceToken.Span.End, text.Lines[open].End));

            if (after.Trim().Length > 0)
            {
                lines.Add((after.Trim(), open, false));
            }

            for (var number = open + 1; number < close; number++)
            {
                var line = text.Lines[number];

                lines.Add((line.ToString(), number, Inside(root, line.Start)));
            }

            var before = text.ToString(TextSpan.FromBounds(text.Lines[close].Start, body.CloseBraceToken.SpanStart));

            if (before.Trim().Length > 0)
            {
                lines.Add((before, close, Inside(root, text.Lines[close].Start)));
            }
        }

        // moved out by as much as all of it stands in, and no more than it was moved in
        var common = lines.Where(l => !l.Inside && l.Text.Trim().Length > 0)
                          .Select(l => l.Text.Length - l.Text.TrimStart(' ', '\t').Length)
                          .DefaultIfEmpty(0)
                          .Min();

        var removed = Math.Min(Indent, common);

        var result = lines.Select(l => l.Inside
                                       ? (l.Text, l.Line, 0)
                                       : l.Text.Trim().Length == 0
                                           ? (string.Empty, l.Line, 0)
                                           : (l.Text[removed..], l.Line, removed))
                          .ToList();

        while (result.Count > 0 && result[0].Item1.Length == 0)
        {
            result.RemoveAt(0);
        }

        while (result.Count > 0 && result[^1].Item1.Length == 0)
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    /// <summary>
    /// Whether a position is inside a token rather than before one: a line
    /// starting there continues a string written over several lines.
    /// </summary>
    private static bool Inside(SyntaxNode root, int position)
    {
        var token = root.FindToken(position);

        return position > token.SpanStart && position < token.Span.End;
    }

    private static string NameOf(MemberDeclarationSyntax member) => member switch
    {
        MethodDeclarationSyntax method => method.Identifier.Text,
        PropertyDeclarationSyntax property => property.Identifier.Text,
        FieldDeclarationSyntax field => string.Join(", ", field.Declaration.Variables.Select(v => v.Identifier.Text)),
        BaseTypeDeclarationSyntax type => type.Identifier.Text,
        ConstructorDeclarationSyntax => "the constructor",
        EventDeclarationSyntax e => e.Identifier.Text,
        EventFieldDeclarationSyntax e => string.Join(", ", e.Declaration.Variables.Select(v => v.Identifier.Text)),
        DelegateDeclarationSyntax d => d.Identifier.Text,
        _ => member.Kind().ToString()
    };

    #endregion

}

/// <summary>
/// A snippet taken out of the class <c>Project</c>.
/// </summary>
/// <param name="Snippet">What lambda.cs holds, or nothing where it could not be taken out</param>
/// <param name="Complaint">Why it could not</param>
/// <param name="Origins">
/// For every line of the snippet, the line of Project.cs it came from (zero based, -1 for a line the way back put
/// in) and how many characters it was moved out by - to say where in Project.cs a problem the compiler found is
/// </param>
public sealed record SnippetUnwrapped(string? Snippet, string? Complaint, IReadOnlyList<(int Line, int Removed)> Origins)
{

    public static SnippetUnwrapped Refused(string complaint) => new(null, complaint, []);

    /// <summary>
    /// Where in Project.cs a position in the snippet is, both one based - or
    /// nothing where the line is not one of Project.cs.
    /// </summary>
    public (int Line, int Column)? Locate(int line, int column)
    {
        if (line < 1 || line > Origins.Count || Origins[line - 1].Line < 0)
        {
            return null;
        }

        var (origin, removed) = Origins[line - 1];

        return (origin + 1, column + removed);
    }

}
