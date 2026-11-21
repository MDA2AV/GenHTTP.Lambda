using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Lambda.Web;

/// <summary>
/// The files of a bundle as they are served: all but those at its root that
/// only the server reads.
/// </summary>
internal sealed class ServedTree(IResourceTree files, IReadOnlyCollection<string> hidden) : IResourceTree
{

    public DateTime? Modified => files.Modified;

    public ValueTask<IResourceNode?> TryGetNodeAsync(PathSegment segment) => files.TryGetNodeAsync(segment);

    public ValueTask<IReadOnlyCollection<IResourceNode>> GetNodes() => files.GetNodes();

    public ValueTask<IResource?> TryGetResourceAsync(PathSegment segment)
        => hidden.Contains(segment.Decode()) ? new((IResource?)null) : files.TryGetResourceAsync(segment);

    public async ValueTask<IReadOnlyCollection<IResource>> GetResources()
        => [.. (await files.GetResources()).Where(r => !hidden.Contains(r.Name ?? string.Empty))];

}
