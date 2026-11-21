using System.Text;

using GenHTTP.Modules.Git;

using GenHTTP.Lambda.Services.Deployment;
using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Git;

/// <summary>
/// The lambda a pushed tree holds.
/// </summary>
/// <param name="Files">The lambda's files, in its order - or nothing where the tree holds none</param>
/// <param name="Stored">The same as a commit keeps them</param>
/// <param name="Contents">What is kept for them besides the tree: the lambda.cs taken out of Project.cs</param>
/// <param name="Project">Project.cs as pushed, to say where in it a problem the compiler found is</param>
/// <param name="Complaint">Why the tree is no lambda</param>
public sealed record ReadTree(IReadOnlyList<VersionFile>? Files, IReadOnlyList<StoredFile> Stored, IReadOnlyDictionary<string, byte[]> Contents,
                                string? Project, string? Complaint)
{

    public static ReadTree Refused(string complaint) => new(null, [], new Dictionary<string, byte[]>(), null, complaint);

}
