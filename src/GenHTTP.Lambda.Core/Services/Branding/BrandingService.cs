using GenHTTP.Lambda.Configuration;
using GenHTTP.Lambda.Data;
using GenHTTP.Lambda.Data.Entities;
using GenHTTP.Lambda.Services.Meta;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Services.Branding;

/// <summary>
/// How the installation presents itself on its pages, and what its operator
/// changes about it.
/// </summary>
/// <remarks>
/// Read for every page served - the bar, the title, the icon of the tab -
/// so held in memory once it was read, the picture with it, and only written
/// through to the database: the name as a setting (<c>branding.name</c>), the
/// logo as a row of its own. What is checked about either is
/// <see cref="BrandChecks"/>'s.
///
/// Where the host does not let the operator brand the installation, it is the
/// platform's own whatever the database holds - a copy of the data of another
/// installation brings nothing of its looks along - and changes are refused.
/// </remarks>
public sealed class BrandingService(IDbContextFactory<LambdaDbContext> databases, LambdaOptions options) : IBrandingService
{
    private const string NameKey = "branding.name";

    private const string LogoPurpose = "logo";

    private readonly Lock _lock = new();

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

            if (_current is { } current)
            {
                return current;
            }

            lock (_lock)
            {
                return _current ??= Read();
            }
        }
    }

    private Brand Read()
    {
        using var database = databases.CreateDbContext();

        var name = database.Settings.AsNoTracking().Where(s => s.Key == NameKey).Select(s => s.Value).FirstOrDefault();

        var logo = database.BrandingPictures.AsNoTracking().FirstOrDefault(p => p.Purpose == LogoPurpose);

        return Brand.Of(name, logo == null ? null : new BrandPicture(logo.Content, logo.Type, logo.Updated));
    }

    #endregion

    #region Changing

    /// <summary>
    /// Names the installation, or gives it back the platform's name where the
    /// name is empty.
    /// </summary>
    public (Brand Brand, string? Previous) Rename(string? name)
    {
        RequireChangeable();

        var given = BrandChecks.Name(name);

        lock (_lock)
        {
            var current = _current ?? Read();

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
    }

    /// <summary>
    /// Gives the installation a logo, or another one.
    /// </summary>
    public Brand ChangeLogo(byte[] content)
    {
        RequireChangeable();

        var type = BrandChecks.Logo(content);

        var logo = new BrandPicture(content, type, DateTime.UtcNow);

        lock (_lock)
        {
            var current = _current ?? Read();

            using var database = databases.CreateDbContext();

            if (database.BrandingPictures.Find(LogoPurpose) is { } existing)
            {
                existing.Type = logo.Type;
                existing.Content = logo.Content;
                existing.Updated = logo.Updated;
            }
            else
            {
                database.BrandingPictures.Add(new BrandingPictureEntity { Purpose = LogoPurpose, Type = logo.Type, Content = logo.Content, Updated = logo.Updated });
            }

            database.SaveChanges();

            return _current = Brand.Of(current.Given, logo);
        }
    }

    /// <summary>
    /// Takes the logo away.
    /// </summary>
    public Brand RemoveLogo()
    {
        RequireChangeable();

        lock (_lock)
        {
            var current = _current ?? Read();

            using var database = databases.CreateDbContext();

            if (database.BrandingPictures.Find(LogoPurpose) is { } existing)
            {
                database.BrandingPictures.Remove(existing);
                database.SaveChanges();
            }

            return _current = Brand.Of(current.Given, null);
        }
    }

    /// <summary>
    /// Gives the installation back the platform's name and mark.
    /// </summary>
    public Brand Reset()
    {
        RequireChangeable();

        lock (_lock)
        {
            using var database = databases.CreateDbContext();

            if (database.Settings.Find(NameKey) is { } name)
            {
                database.Settings.Remove(name);
            }

            if (database.BrandingPictures.Find(LogoPurpose) is { } logo)
            {
                database.BrandingPictures.Remove(logo);
            }

            database.SaveChanges();

            return _current = Brand.Platform;
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
