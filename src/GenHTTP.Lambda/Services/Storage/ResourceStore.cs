using System.Security.Cryptography;

using GenHTTP.Lambda.Services.Deployment.Model;

namespace GenHTTP.Lambda.Services.Storage;

/// <summary>
/// Keeps the resources of one lambda as files below <c>{data}/code/{id}/resources</c>,
/// each named by the SHA-256 of what it holds, in a folder of its first two digits.
/// </summary>
/// <remarks>
/// Written through a file beside it and moved into place, so that a name in
/// the store always holds all of what it names - whoever finds it there may
/// copy it out at once.
/// </remarks>
internal sealed class ResourceStore(string root) : IResourceStore
{

    #region Functionality

    public StoredContent Keep(ReadOnlySpan<byte> content)
    {
        var sha256 = Convert.ToHexStringLower(SHA256.HashData(content));

        var path = PathOf(sha256);

        if (File.Exists(path))
        {
            // kept already, and named again: young again too, so that a
            // collection does not take it before what names it is written
            File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
        }
        else
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var staging = $"{path}.{Guid.NewGuid():N}.keeping";

            using (var file = new FileStream(staging, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                file.Write(content);
            }

            try
            {
                File.Move(staging, path, false);
            }
            catch (IOException) when (File.Exists(path))
            {
                // the same, kept by another save in the meantime
                File.Delete(staging);
            }
        }

        return new StoredContent(sha256, content.Length, this);
    }

    public bool Has(string sha256) => StoredContent.IsHash(sha256) && File.Exists(PathOf(sha256));

    public Stream Open(string sha256) => new FileStream(PathOf(sha256), FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);

    public void CopyTo(string sha256, string path) => File.Copy(PathOf(sha256), path, true);

    /// <summary>
    /// Removes what nothing names any more and was last named before the
    /// given time, and says how much that was.
    /// </summary>
    /// <remarks>
    /// The time is a grace for what is being saved right now: its resources
    /// are kept before the version that names them is written, and a
    /// collection in between must not take them.
    /// </remarks>
    public (int Files, long Bytes) Collect(IReadOnlySet<string> named, DateTime before)
    {
        if (!Directory.Exists(root))
        {
            return (0, 0);
        }

        int files = 0;

        long bytes = 0;

        foreach (var file in new DirectoryInfo(root).EnumerateFiles("*", SearchOption.AllDirectories))
        {
            // a file still being kept is not one yet, and one left behind by
            // a save that broke off is a name nothing could know
            var orphan = file.Name.EndsWith(".keeping", StringComparison.Ordinal);

            if (!orphan && named.Contains(file.Name))
            {
                continue;
            }

            if (file.LastWriteTimeUtc >= before)
            {
                continue;
            }

            try
            {
                var length = file.Length;

                file.Delete();

                files++;
                bytes += length;
            }
            catch (IOException)
            {
                // being read right now; the next collection takes it
            }
        }

        return (files, bytes);
    }

    #endregion

    #region Helpers

    private string PathOf(string sha256)
        => StoredContent.IsHash(sha256)
         ? Path.Combine(root, sha256[..2], sha256)
         : throw new ArgumentException($"'{sha256}' is not the hash of a resource.", nameof(sha256));

    #endregion

}
