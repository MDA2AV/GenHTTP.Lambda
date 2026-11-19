using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Integrations.Files;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages, and what its operator
/// changes about it.
/// </summary>
/// <remarks>
/// Read for every page served - the bar, the title, the icon of the tab -
/// so read once as the platform starts (<see cref="LoadAsync"/>), held in
/// memory with the picture, and only written through: the name as a setting (<c>branding.name</c>), what the logo is as
/// a row of its own, and the logo itself as a file of the files integration
/// (<c>branding/logo</c>) - no picture is kept in a database. What is checked
/// about either is <see cref="BrandChecks"/>'s.
///
/// Where the host does not let the operator brand the installation, it is the
/// platform's own whatever the database holds - a copy of the data of another
/// installation brings nothing of its looks along - and changes are refused.
/// </remarks>
public sealed class BrandingService(IDbContextFactory<LambdaDbContext> databases, IFileProvider files, LambdaOptions options) : IBrandingService
{
    private const string NameKey = "branding.name";

    private const string LogoPurpose = "logo";

    /// <summary>
    /// Who is changing the brand: one at a time, across the writing of its
    /// logo.
    /// </summary>
    private readonly SemaphoreSlim _turn = new(1, 1);

    private Brand? _current;

    #region Reading

    /// <summary>
    /// Whether the operator may give the installation a name and a logo here.
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
        string? name;

        BrandingPictureEntity? logo;

        using (var database = databases.CreateDbContext())
        {
            name = database.Settings.AsNoTracking().Where(s => s.Key == NameKey).Select(s => s.Value).FirstOrDefault();

            logo = database.BrandingPictures.AsNoTracking().FirstOrDefault(p => p.Purpose == LogoPurpose);
        }

        // the row's own bytes where the logo was not moved out of it yet
        var content = logo == null ? null : await files.ReadAsync(FileStore.Branding, PictureNames.Branding(LogoPurpose), cancellation) ?? (logo.Content.Length > 0 ? logo.Content : null);

        return Brand.Of(name, logo == null || content == null ? null : new BrandPicture(content, logo.Type, logo.Updated));
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

            using var database = databases.CreateDbContext();

            var existing = database.Settings.Find(NameKey);

            if (given == null)
            {
                if (existing != null)
                {
                    database.Settings.Remove(existing);
                }
            }
            else if (existing == null)
            {
                database.Settings.Add(new SettingEntity { Key = NameKey, Value = given });
            }
            else
            {
                existing.Value = given;
            }

            database.SaveChanges();

            return (_current = Brand.Of(given, current.Logo), current.Given);
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

            using var database = databases.CreateDbContext();

            if (database.BrandingPictures.Find(LogoPurpose) is { } existing)
            {
                existing.Type = logo.Type;
                existing.Content = [];
                existing.Updated = logo.Updated;
            }
            else
            {
                database.BrandingPictures.Add(new BrandingPictureEntity { Purpose = LogoPurpose, Type = logo.Type, Updated = logo.Updated });
            }

            database.SaveChanges();

            return _current = Brand.Of(current.Given, logo);
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

            using (var database = databases.CreateDbContext())
            {
                if (database.BrandingPictures.Find(LogoPurpose) is { } existing)
                {
                    database.BrandingPictures.Remove(existing);
                    database.SaveChanges();
                }
            }

            await files.DeleteAsync(FileStore.Branding, PictureNames.Branding(LogoPurpose), cancellation);

            return _current = Brand.Of(current.Given, null);
        }
        finally
        {
            _turn.Release();
        }
    }

    /// <summary>
    /// Gives the installation back the platform's name and mark.
    /// </summary>
    public async ValueTask<Brand> ResetAsync(CancellationToken cancellation = default)
    {
        RequireChangeable();

        await _turn.WaitAsync(cancellation);

        try
        {
            using (var database = databases.CreateDbContext())
            {
                if (database.Settings.Find(NameKey) is { } name)
                {
                    database.Settings.Remove(name);
                }

                if (database.BrandingPictures.Find(LogoPurpose) is { } logo)
                {
                    database.BrandingPictures.Remove(logo);
                }

                database.SaveChanges();
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
            throw LambdaException.NotFound("The name and the logo of this installation are its host's, and are not changed here.");
        }
    }

    #endregion

}
