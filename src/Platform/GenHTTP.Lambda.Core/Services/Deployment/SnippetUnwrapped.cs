using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using GenHTTP.Lambda.Services.Deployment.Compilation;

namespace GenHTTP.Lambda.Services.Deployment;

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
