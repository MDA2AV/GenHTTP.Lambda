using GenHTTP.Lambda.Services.Deployment.Model;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using GenHTTP.Lambda.Integrations.Databases;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// Where a name was declared, so the editor can go there.
/// </summary>
/// <param name="File">The file of the user it was declared in</param>
/// <param name="Line">The line it is on, counting from zero</param>
/// <param name="Column">The column it starts at, counting from zero</param>
/// <param name="Length">How long the name itself is</param>
public sealed record ResolvedDefinition(string File, int Line, int Column, int Length);
