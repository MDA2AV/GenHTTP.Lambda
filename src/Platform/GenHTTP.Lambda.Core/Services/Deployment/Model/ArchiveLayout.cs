using System.IO.Compression;
using System.Text;

using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.Storage;

namespace GenHTTP.Lambda.Services.Deployment.Model;

/// <summary>
/// How the files of a lambda are laid out in an archive.
/// </summary>
public enum ArchiveLayout
{

    /// <summary>Named as the lambda names them: lambda.cs, its code, resources/.</summary>
    Lambda,

    /// <summary>
    /// Where a clone of the lambda has them: Project.cs for lambda.cs, the
    /// other .cs files named the .NET way, and the rest as the lambda names it.
    /// </summary>
    Project

}
