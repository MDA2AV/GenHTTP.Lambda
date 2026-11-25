using System.Security.Cryptography;
using System.Text;

using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Infrastructure;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.SharedFiles;

/// <summary>
/// Where the shared files are kept: a row each saying what one is, and its
/// content with the files integration, under the SHA-256 of what it holds.
/// </summary>
/// <remarks>
/// By its content rather than its name, so replacing a file writes another
/// one and then says which is meant: whoever reads while it is replaced reads
/// the one or the other, never a part of each, and an address that names the
/// content (<see cref="SharedFile.VersionedPath"/>) never answers with
/// another. Two names of the same content are one file there.
///
/// Read from the records with every call rather than from memory. They are
/// asked for rarely - a browser keeps what it was given by such an address -
/// and whatever changes them on another server is seen here at once.
/// </remarks>
public sealed class SharedFileStore(IDbContextFactory<LambdaDbContext> databases, IFileProvider files) : ISharedFiles
{

    /// <summary>
    /// Who is changing the files: one at a time, so how many there are and
    /// how large they are together is counted once.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    #region Reading

    public async ValueTask<IReadOnlyList<SharedFile>> ListAsync(CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        var rows = await database.SharedFiles.AsNoTracking().ToListAsync(cancellation);

        return [.. rows.Select(Describe).OfType<SharedFile>().OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)];
    }

    public async ValueTask<SharedFile?> FindAsync(string name, CancellationToken cancellation = default)
    {
        await using var database = await databases.CreateDbContextAsync(cancellation);

        return Describe(await database.SharedFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Name == name, cancellation));
    }

    public ValueTask<byte[]?> ReadAsync(SharedFile file, CancellationToken cancellation = default)
        => files.ReadAsync(FileStore.Shared, file.Hash, cancellation);

    public async ValueTask<string> ReadTextAsync(string name, CancellationToken cancellation = default)
    {
        var file = await FindAsync(name, cancellation) ?? throw Missing(name);

        if (!file.Type.Readable)
        {
            throw LambdaException.Invalid($"'{name}' is a {SharedFileKindNames.Name(file.Type.Kind)} ({file.Type.Type}) rather than text: a page links to it at {file.Path} of this platform.");
        }

        if (file.Bytes > SharedFileChecks.MaxReadBytes)
        {
            throw LambdaException.Invalid($"'{name}' is {file.Bytes:N0} bytes, more than a tool answers with: fetch it from {file.Path} of this platform.");
        }

        return Encoding.UTF8.GetString(await ReadAsync(file, cancellation) ?? throw Missing(name));
    }

    /// <summary>
    /// A row as a file - or nothing for one of a type no version of the
    /// platform shares any more.
    /// </summary>
    private static SharedFile? Describe(SharedFileEntity? row)
    {
        if (row == null || SharedFileType.Of(row.Name) is not { } type)
        {
            return null;
        }

        var font = row is { FontFrom: { } from, FontTo: { } to } ? new FontFace(row.FontFamily ?? "Unnamed", from, to, row.FontItalic ?? false, row.FontLetters) : null;

        return new SharedFile(row.Name, type, row.Bytes, row.Hash, row.Updated, font);
    }

    internal static LambdaException Missing(string name) => LambdaException.NotFound($"There is no shared file called '{name}'.");

    #endregion

    #region Changing

    /// <summary>
    /// Keeps a file, in place of the one of that name.
    /// </summary>
    /// <returns>The file as it is kept, and whether there was one before</returns>
    public async ValueTask<(SharedFile File, bool Replaced)> WriteAsync(string name, byte[] content, CancellationToken cancellation = default)
    {
        var type = SharedFileChecks.Name(name);

        SharedFileChecks.Size(content.Length);

        // unpacked and hashed away from the thread that serves the request:
        // two megabytes of a font take a while to look into
        var (font, hash) = await Offload.Run(() => (SharedFileChecks.Content(name, type, content), Convert.ToHexStringLower(SHA256.HashData(content))), cancellation);

        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            var rows = await database.SharedFiles.ToListAsync(cancellation);

            var existing = rows.Find(f => f.Name == name);

            Admit(name, content.Length, rows, existing);

            await files.WriteAsync(FileStore.Shared, hash, content, cancellation);

            // the content is written, so what says it is there is written as well, whoever cancels
            var replaced = existing?.Hash;

            if (existing == null)
            {
                database.SharedFiles.Add(existing = new SharedFileEntity { Name = name, Hash = hash });
                rows.Add(existing);
            }

            existing.Bytes = content.Length;
            existing.Hash = hash;
            existing.Updated = DateTime.UtcNow;
            existing.FontFamily = font?.Family;
            existing.FontFrom = font?.From;
            existing.FontTo = font?.To;
            existing.FontItalic = font?.Italic;
            existing.FontLetters = font?.Letters;

            await database.SaveChangesAsync(CancellationToken.None);

            if (replaced != null)
            {
                await ForgetAsync(replaced, rows);
            }

            return (Describe(existing)!, replaced != null);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Removes a file.
    /// </summary>
    /// <returns>The file that was there, or nothing where there was none</returns>
    public async ValueTask<SharedFile?> DeleteAsync(string name, CancellationToken cancellation = default)
    {
        await _turn.WaitAsync(cancellation);

        try
        {
            await using var database = await databases.CreateDbContextAsync(cancellation);

            var rows = await database.SharedFiles.ToListAsync(cancellation);

            if (rows.Find(f => f.Name == name) is not { } existing)
            {
                return null;
            }

            database.SharedFiles.Remove(existing);

            await database.SaveChangesAsync(cancellation);

            rows.Remove(existing);

            await ForgetAsync(existing.Hash, rows);

            return Describe(existing);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Refuses a file there is no room for, or whose name is another's but
    /// for its case - which on some disks, and to most people, is one name.
    /// </summary>
    private static void Admit(string name, int bytes, List<SharedFileEntity> rows, SharedFileEntity? existing)
    {
        if (existing == null)
        {
            if (rows.Find(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase)) is { } alike)
            {
                throw LambdaException.Conflict($"There is a shared file called '{alike.Name}' already, which differs only in case: replace that one, or choose another name.");
            }

            if (rows.Count >= SharedFileChecks.MaxFiles)
            {
                throw LambdaException.Invalid($"There are {SharedFileChecks.MaxFiles} shared files, as many as there may be: delete one first.");
            }
        }

        if (rows.Sum(f => (long)f.Bytes) - (existing?.Bytes ?? 0) + bytes > SharedFileChecks.MaxTotalBytes)
        {
            throw LambdaException.Invalid($"The shared files would be larger than {SharedFileChecks.MaxTotalBytes / 1024 / 1024} MB together: delete one first.");
        }
    }

    /// <summary>
    /// Lets go of a content no file holds any more.
    /// </summary>
    private async ValueTask ForgetAsync(string hash, List<SharedFileEntity> rows)
    {
        if (rows.TrueForAll(f => f.Hash != hash))
        {
            await files.DeleteAsync(FileStore.Shared, hash, CancellationToken.None);
        }
    }

    #endregion

}
