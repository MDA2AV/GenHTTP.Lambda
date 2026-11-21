using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Security.Cryptography;

using System.Data.Common;

using GenHTTP.Api.Content;
using GenHTTP.Lambda.Integrations.Databases;
using GenHTTP.Lambda.Services.Deployment.Model;
using GenHTTP.Lambda.Services.Databases;
using GenHTTP.Lambda.Services.Secrets;
using GenHTTP.Lambda.Services.Workspace;

using Microsoft.EntityFrameworkCore;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Lambda.Services.Deployment.Compilation;

/// <summary>
/// What the compiler needs to build a snippet.
/// </summary>
/// <param name="Files">The source files as written by the user</param>
/// <param name="Resources">The directory the lambda's resources were written to</param>
/// <param name="AssemblyDirectory">Where the generated assembly is written to</param>
/// <param name="Name">A readable prefix for the generated namespace and assembly</param>
/// <param name="Run">Whether the result should be loaded and invoked, or only checked</param>
/// <param name="Settings">Whether the lambda has a workspace and whether it may connect to other servers, compiled into it</param>
/// <param name="Binding">How the lambda is bound to its database, as the database integration says</param>
/// <param name="Secrets">What the lambda reads its secrets with, once it runs</param>
/// <param name="Database">What the lambda connects to its database with, once it runs</param>
/// <param name="Context">What a context of Entity Framework is configured with on its connection, once it runs</param>
/// <param name="Workspace">What it reads and writes its files through, once it runs</param>
internal sealed record CompilationRequest(IReadOnlyList<LambdaFile> Files, string Resources, string AssemblyDirectory, string Name, bool Run,
                                          BuildSettings Settings, DatabaseBinding Binding, SecretReader? Secrets = null,
                                          DatabaseConnector? Database = null, Action<DbContextOptionsBuilder, DbConnection>? Context = null,
                                          WorkspaceBinding? Workspace = null);
