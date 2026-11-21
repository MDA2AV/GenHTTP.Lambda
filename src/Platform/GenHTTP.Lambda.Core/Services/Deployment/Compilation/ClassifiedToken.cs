using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using GenHTTP.Lambda.Integrations.Databases;

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
