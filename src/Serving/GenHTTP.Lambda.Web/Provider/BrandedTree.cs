using System.Buffers;
using System.Text;

using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

using GenHTTP.Lambda.Services.Branding;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The files of a bundle, with its index page as the installation presents
/// itself (see <see cref="PageBranding"/>) - for the framework's single page
/// application to serve, which reads its index page from the tree it is given.
/// </summary>
internal sealed class BrandedTree(IResourceTree files, IResource index) : IResourceTree
{
    private static readonly PathSegment Index = new("index.html");

    public DateTime? Modified => files.Modified;

    public ValueTask<IResourceNode?> TryGetNodeAsync(PathSegment segment) => files.TryGetNodeAsync(segment);

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => files.GetNodes();

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment)
        => segment == Index ? new(index) : files.TryGetResourceAsync(segment);

    public ValueTask<IReadOnlyCollection<IResource>> GetResources() => files.GetResources();

}
