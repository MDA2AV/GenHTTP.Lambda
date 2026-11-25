using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Meta;
using GenHTTP.Lambda.Services.SharedFiles;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages, and what its operator
/// changes about it.
/// </summary>
/// <remarks>
/// Read for every page served - the bar, the title, the icon of the tab -
/// so read once as the platform starts (<see cref="LoadAsync"/>), held in
/// memory with the picture, and only written through: the name and the
/// colours as settings (<c>branding.name</c>, <c>branding.accent</c>,
/// <c>branding.second</c>), what the logo is as a row of its own, and the logo
/// itself as a file of the files integration (<c>branding/logo</c>) - no
/// picture is kept in a database. A font is a setting too, the names of the
/// shared files it is made of, a line each (<c>branding.font.text</c>,
/// <c>branding.font.headings</c>): the files themselves are shared ones, so an
/// app may be set in them as well. What is checked about any of it is
/// <see cref="BrandChecks"/>'.
///
/// Where the host does not let the operator brand the installation, it is the
/// platform's own whatever the database holds - a copy of the data of another
/// installation brings nothing of its looks along - and changes are refused.
/// </remarks>
public sealed class BrandingService(IDbContextFactory<LambdaDbContext> databases, IFileProvider files, ISharedFiles shared, LambdaOptions options) : IBrandingService
{
    private const string NameKey = "branding.name";

    private const string AccentKey = "branding.accent";

    private const string SecondKey = "branding.second";

    private const string TextFontKey = "branding.font.text";

    private const string HeadingFontKey = "branding.font.headings";

    /// <summary>
    /// Every setting that is part of the brand.
    /// </summary>
    private static readonly List<string> Keys = [NameKey, AccentKey, SecondKey, TextFontKey, HeadingFontKey];

    private const string LogoPurpose = "logo";

    /// <summary>
    /// Who is changing the brand: one at a time, across the writing of its
    /// logo.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private Brand? _current;

    #region Reading

    /// <summary>
    /// Whether the operator may brand the installation here.
    /// </summary>
    public bool Changeable => options.Branding;

    /// <summary>
    /// How the installation presents itself right now.
    /// </summary>
    public Brand Current
    {
        get
        {
            if (!options.Branding)
            {
                return Brand.Platform;
            }

            return _current ?? throw new InvalidOperationException("The brand of the installation is read as the platform starts, which it has not.");
        }
    }

    /// <summary>
    /// Reads how the installation presents itself, as the platform starts -
    /// before the first page asks.
    /// </summary>
    public async ValueTask LoadAsync(CancellationToken cancellation = default)
    {
        if (options.Branding)
        {
            _current = await ReadAsync(cancellation);
        }
    }

    private async ValueTask<Brand> ReadAsync(CancellationToken cancellation)
    {
        Dictionary<string, string> settings;

        BrandingPictureEntity? logo;

        await using (var database = await databases.CreateDbContextAsync(cancellation))
        {
            settings = await database.Settings.AsNoTracking().Where(s => Keys.Contains(s.Key)).ToDictionaryAsync(s => s.Key, s => s.Value, cancellation);

            logo = await database.BrandingPictures.AsNoTracking().FirstOrDefaultAsync(p => p.Purpose == LogoPurpose, cancellation);
        }

        // the row's own bytes where the logo was not moved out of it yet
        var content = logo == null ? null : await files.ReadAsync(FileStore.Branding, PictureNames.Branding(LogoPurpose), cancellation) ?? (logo.Content.Length > 0 ? logo.Content : null);

        var picture = logo == null || content == null ? null : new BrandPicture(content, logo.Type, logo.Updated);

        var look = BrandLook.Of(Color(settings, AccentKey), Color(settings, SecondKey),
                                await FilesAsync(settings, TextFontKey, cancellation), await FilesAsync(settings, HeadingFontKey, cancellation));

        return Brand.Of(settings.GetValueOrDefault(NameKey), picture, look);
    }

    private static BrandColor? Color(Dictionary<string, string> settings, string key)
        => BrandColor.TryParse(settings.GetValueOrDefault(key), out var color) ? color : null;

    /// <summary>
    /// The files of a font as they are now - without one that is gone, whose
    /// weight is then drawn from the others.
    /// </summary>
    private async ValueTask<IReadOnlyList<SharedFile>> FilesAsync(Dictionary<string, string> settings, string key, CancellationToken cancellation)
    {
        var found = new List<SharedFile>();

        foreach (var name in settings.GetValueOrDefault(key)?.Split('\n', StringSplitOptions.RemoveEmptyEntries) ?? [])
        {
            if (await shared.FindAsync(name, cancellation) is { Font: not null } file)
            {
                found.Add(file);
            }
        }

        return found;
    }

    #endregion

    #region Changing

    /// <summary>
    /// Names the installation, or gives it back the platform's name where the
    /// name is empty.
    /// </summary>
    public async ValueTask<(Brand Brand, string? Previous)> RenameAsync(string? name, CancellationToken cancellation = default)
    {
        RequireChangeable();

        var given = BrandChecks.Name(name);

        await _turn.WaitAsync(cancellation);

        try
        {
            var current = Current;

            await using var database = await databases.CreateDbContextAsync(cancellation);

            await KeepAsync(database, NameKey, given, cancellation);

            await database.SaveChangesAsync(cancellation);

            return (_current = current.Named(given), current.Given);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Gives the installation a logo, or another one.
    /// </summary>
    public async ValueTask<Brand> ChangeLogoAsync(byte[] content, CancellationToken cancellation = default)
    {
        RequireChangeable();

        var type = BrandChecks.Logo(content);

        var logo = new BrandPicture(content, type, DateTime.UtcNow);

        await _turn.WaitAsync(cancellation);

        try
        {
            var current = Current;

            await files.WriteAsync(FileStore.Branding, PictureNames.Branding(LogoPurpose), logo.Content, cancellation);

            // the logo is written, so what says what it is is written as well, whoever cancels
            await using var database = await databases.CreateDbContextAsync(CancellationToken.None);

            if (await database.BrandingPictures.FindAsync([LogoPurpose], CancellationToken.None) is { } existing)
            {
                existing.Type = logo.Type;
                existing.Content = [];
                existing.Updated = logo.Updated;
            }
            else
            {
                database.BrandingPictures.Add(new BrandingPictureEntity { Purpose = LogoPurpose, Type = logo.Type, Updated = logo.Updated });
            }

            await database.SaveChangesAsync(CancellationToken.None);

            return _current = current.With(logo);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Takes the logo away.
    /// </summary>
    public async ValueTask<Brand> RemoveLogoAsync(CancellationToken cancellation = default)
    {
        RequireChangeable();

        await _turn.WaitAsync(cancellation);

        try
        {
            var current = Current;

            await using (var database = await databases.CreateDbContextAsync(cancellation))
            {
                if (await database.BrandingPictures.FindAsync([LogoPurpose], cancellation) is { } existing)
                {
                    database.BrandingPictures.Remove(existing);
                    await database.SaveChangesAsync(cancellation);
                }
            }

            await files.DeleteAsync(FileStore.Branding, PictureNames.Branding(LogoPurpose), cancellation);

            return _current = current.With((BrandPicture?)null);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Draws the pages in the operator's colours.
    /// </summary>
    public async ValueTask<(Brand Brand, BrandLook Previous)> ColorAsync(BrandColor? accent, BrandColor? second, CancellationToken cancellation = default)
    {
        RequireChangeable();

        await _turn.WaitAsync(cancellation);

        try
        {
            var current = Current;

            await using var database = await databases.CreateDbContextAsync(cancellation);

            await KeepAsync(database, AccentKey, accent?.Hex, cancellation);
            await KeepAsync(database, SecondKey, second?.Hex, cancellation);

            await database.SaveChangesAsync(cancellation);

            return (_current = current.With(BrandLook.Of(accent, second, current.Look.TextFont, current.Look.HeadingFont)), current.Look);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Sets the pages in the operator's fonts.
    /// </summary>
    public async ValueTask<(Brand Brand, BrandLook Previous)> FontAsync(IReadOnlyList<string> text, IReadOnlyList<string> headings, CancellationToken cancellation = default)
    {
        RequireChangeable();

        await _turn.WaitAsync(cancellation);

        try
        {
            var current = Current;

            var look = await ComposeAsync(current.Look.Accent, current.Look.Second, text, headings, cancellation);

            await using var database = await databases.CreateDbContextAsync(cancellation);

            await KeepAsync(database, TextFontKey, Lines(look.TextFont), cancellation);
            await KeepAsync(database, HeadingFontKey, Lines(look.HeadingFont), cancellation);

            await database.SaveChangesAsync(cancellation);

            return (_current = current.With(look), current.Look);
        }
        finally
        {
            _turn.Release();
        }
    }

    public async ValueTask<BrandLook> ComposeAsync(BrandColor? accent, BrandColor? second, IReadOnlyList<string> text, IReadOnlyList<string> headings,
                                                   CancellationToken cancellation = default)
        => BrandLook.Of(accent, second, await FontOfAsync(text, "text", cancellation), await FontOfAsync(headings, "heading", cancellation));

    public async ValueTask RefreshAsync(CancellationToken cancellation = default)
    {
        if (!options.Branding)
        {
            return;
        }

        await _turn.WaitAsync(cancellation);

        try
        {
            _current = await ReadAsync(cancellation);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// The files a font is made of, or why it cannot be made of them.
    /// </summary>
    private async ValueTask<IReadOnlyList<SharedFile>> FontOfAsync(IReadOnlyList<string> names, string what, CancellationToken cancellation)
    {
        var found = new List<SharedFile>();

        foreach (var name in names.Distinct(StringComparer.Ordinal))
        {
            var file = await shared.FindAsync(name, cancellation)
                    ?? throw LambdaException.Invalid($"There is no shared file called '{name}' for the {what} font: share it first.");

            if (file.Font == null)
            {
                throw LambdaException.Invalid($"'{name}' is no font, so the {what} font cannot be made of it.");
            }

            found.Add(file);
        }

        if (found.Count > BrandChecks.MaxFontFiles)
        {
            throw LambdaException.Invalid($"The {what} font must not be made of more than {BrandChecks.MaxFontFiles} files.");
        }

        return found;
    }

    private static string? Lines(IReadOnlyList<SharedFile> files) => files.Count == 0 ? null : string.Join('\n', files.Select(f => f.Name));

    /// <summary>
    /// Keeps a setting, or lets go of it where there is nothing to keep.
    /// </summary>
    private static async ValueTask KeepAsync(LambdaDbContext database, string key, string? value, CancellationToken cancellation)
    {
        var existing = await database.Settings.FindAsync([key], cancellation);

        if (value == null)
        {
            if (existing != null)
            {
                database.Settings.Remove(existing);
            }
        }
        else if (existing == null)
        {
            database.Settings.Add(new SettingEntity { Key = key, Value = value });
        }
        else
        {
            existing.Value = value;
        }
    }

    /// <summary>
    /// Gives the installation back the platform's name, mark, colours and
    /// fonts. The files of the fonts are shared ones, and stay.
    /// </summary>
    public async ValueTask<Brand> ResetAsync(CancellationToken cancellation = default)
    {
        RequireChangeable();

        await _turn.WaitAsync(cancellation);

        try
        {
            await using (var database = await databases.CreateDbContextAsync(cancellation))
            {
                foreach (var setting in await database.Settings.Where(s => Keys.Contains(s.Key)).ToListAsync(cancellation))
                {
                    database.Settings.Remove(setting);
                }

                if (await database.BrandingPictures.FindAsync([LogoPurpose], cancellation) is { } logo)
                {
                    database.BrandingPictures.Remove(logo);
                }

                await database.SaveChangesAsync(cancellation);
            }

            await files.DeleteAsync(FileStore.Branding, PictureNames.Branding(LogoPurpose), cancellation);

            return _current = Brand.Platform;
        }
        finally
        {
            _turn.Release();
        }
    }

    private void RequireChangeable()
    {
        if (!options.Branding)
        {
            throw LambdaException.NotFound("How this installation presents itself is its host's, and is not changed here.");
        }
    }

    #endregion

}
