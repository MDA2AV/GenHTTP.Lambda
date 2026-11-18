using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace GenHTTP.Lambda.Integrations.PostgreSql;

/// <summary>
/// Has Entity Framework name the tables and columns of a lambda's context in
/// lower case.
/// </summary>
/// <remarks>
/// A lambda's migrations are plain SQL that every database accepts, which
/// writes a name without quotes - and PostgreSQL folds such a name to lower
/// case, while Entity Framework quotes the name of a property as it is
/// written. SQLite does not mind the case of a name, and code written for it
/// relies on that: <c>Title</c> is the column <c>title</c>. Folding what
/// Entity Framework asks for the same way keeps that code what it was.
/// </remarks>
internal sealed class LowerCaseNames : IDbContextOptionsExtension
{
    private DbContextOptionsExtensionInfo? _info;

    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
        => new EntityFrameworkServicesBuilder(services).TryAdd<IConventionSetPlugin, Plugin>();

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "LowerCaseNames ";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) => debugInfo["LowerCaseNames"] = "1";

    }

    private sealed class Plugin : IConventionSetPlugin
    {

        public ConventionSet ModifyConventions(ConventionSet conventionSet)
        {
            conventionSet.ModelFinalizingConventions.Add(new Folding());
            return conventionSet;
        }

    }

    /// <summary>
    /// Folds the names once the model is complete, so what a context said
    /// about them itself is folded as well.
    /// </summary>
    private sealed class Folding : IModelFinalizingConvention
    {

        public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
        {
            foreach (var entity in modelBuilder.Metadata.GetEntityTypes().Cast<IMutableEntityType>())
            {
                if (entity.GetTableName() is { } table && Folds(table))
                {
                    entity.SetTableName(table.ToLowerInvariant());
                }

                if (entity.GetViewName() is { } view && Folds(view))
                {
                    entity.SetViewName(view.ToLowerInvariant());
                }

                foreach (var property in entity.GetProperties())
                {
                    if (property.GetColumnName() is { } column && Folds(column))
                    {
                        property.SetColumnName(column.ToLowerInvariant());
                    }
                }
            }
        }

        private static bool Folds(string name) => name != name.ToLowerInvariant();

    }

}
