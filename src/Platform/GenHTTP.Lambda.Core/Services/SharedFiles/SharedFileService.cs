using GenHTTP.Lambda.Services.Branding;
using GenHTTP.Lambda.Services.Guide;
using GenHTTP.Lambda.Services.Meta;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// The shared files as the operator manages them: kept by
/// <see cref="SharedFileStore"/>, and here held together with what of the
/// installation uses them.
/// </summary>
/// <remarks>
/// A file that is used is not deleted: the pages would lose their font, and
/// agents be told to read what is not there. It may be replaced - that is
/// what most changes to one are - and the pages are then drawn with the new
/// one.
/// </remarks>
public sealed class SharedFileService(SharedFileStore store, IBrandingService branding, IGuideService guide) : ISharedFileService
{

    #region Functionality

    public async ValueTask<IReadOnlyList<SharedFileUsage>> ListAsync(CancellationToken cancellation = default)
        => [.. (await store.ListAsync(cancellation)).Select(Used)];

    public async ValueTask<(SharedFileUsage File, bool Replaced)> WriteAsync(string name, byte[] content, CancellationToken cancellation = default)
    {
        var (file, replaced) = await store.WriteAsync(name, content, cancellation);

        var used = Used(file);

        // what the pages are sent with names the content, which is another now
        if (replaced && used.Uses.Any(u => u is SharedFileUse.TextFont or SharedFileUse.HeadingFont))
        {
            await branding.RefreshAsync(CancellationToken.None);
        }

        return (used, replaced);
    }

    public async ValueTask<SharedFile> DeleteAsync(string name, CancellationToken cancellation = default)
    {
        var file = await store.FindAsync(name, cancellation) ?? throw SharedFileStore.Missing(name);

        if (Used(file).Uses is { Count: > 0 } uses)
        {
            throw LambdaException.Conflict($"'{name}' is used by {string.Join(" and ", uses.Select(SharedFileUseNames.Words))}: take it out there first, or replace it.");
        }

        return await store.DeleteAsync(name, cancellation) ?? throw SharedFileStore.Missing(name);
    }

    private SharedFileUsage Used(SharedFile file)
    {
        var look = branding.Current.Look;

        var uses = new List<SharedFileUse>();

        if (look.TextFont.Any(f => f.Name == file.Name))
        {
            uses.Add(SharedFileUse.TextFont);
        }

        if (look.HeadingFont.Any(f => f.Name == file.Name))
        {
            uses.Add(SharedFileUse.HeadingFont);
        }

        if (guide.SharedFiles.Contains(file.Name))
        {
            uses.Add(SharedFileUse.Guide);
        }

        return new SharedFileUsage(file, uses);
    }

    #endregion

}
