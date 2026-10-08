using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Entity framework access to the SQLite file holding the lambda meta data.
/// The schema itself is maintained by Evolve, see <see cref="Migrator" />.
/// </summary>
public sealed class LambdaDbContext(DbContextOptions<LambdaDbContext> options) : DbContext(options)
{

    public DbSet<LambdaEntity> Lambdas => Set<LambdaEntity>();

    public DbSet<DeploymentEntity> Deployments => Set<DeploymentEntity>();

    public DbSet<EventEntity> Events => Set<EventEntity>();

    public DbSet<ActivationEntity> Activations => Set<ActivationEntity>();

    public DbSet<ShowcaseEntity> Showcases => Set<ShowcaseEntity>();

    public DbSet<SourceEntity> Sources => Set<SourceEntity>();

    public DbSet<SettingEntity> Settings => Set<SettingEntity>();

    public DbSet<DataStoreEntity> DataStores => Set<DataStoreEntity>();

    public DbSet<FeatureEntity> Features => Set<FeatureEntity>();

    public DbSet<SecretEntity> Secrets => Set<SecretEntity>();

    /// <summary>
    /// Every date is written in UTC, and read back as UTC.
    /// </summary>
    /// <remarks>
    /// SQLite keeps a date as text and forgets which zone it was in, so it
    /// came back unspecified and went out as JSON without a zone - which a
    /// browser reads as its own local time, and a version saved a minute ago
    /// in Vienna was shown as two hours old.
    /// </remarks>
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<UtcConverter>();
        builder.Properties<DateTime?>().HaveConversion<NullableUtcConverter>();
    }

    private sealed class UtcConverter() : ValueConverter<DateTime, DateTime>(
        value => value,
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private sealed class NullableUtcConverter() : ValueConverter<DateTime?, DateTime?>(
        value => value,
        value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var lambdas = builder.Entity<LambdaEntity>();

        lambdas.ToTable("lambdas");

        lambdas.HasKey(l => l.Id);

        lambdas.Property(l => l.Id).HasColumnName("id");
        lambdas.Property(l => l.PublicKey).HasColumnName("public_key");
        lambdas.Property(l => l.PrivateKey).HasColumnName("private_key");
        lambdas.Property(l => l.Tier).HasColumnName("tier").HasConversion<string>();
        lambdas.Property(l => l.ActiveVersion).HasColumnName("active_version");
        lambdas.Property(l => l.Created).HasColumnName("created");
        lambdas.Property(l => l.Modified).HasColumnName("modified");
        lambdas.Property(l => l.Deployed).HasColumnName("deployed");
        lambdas.Property(l => l.LastSeen).HasColumnName("last_seen");
        lambdas.Property(l => l.Domain).HasColumnName("domain");
        lambdas.Property(l => l.View).HasColumnName("editor_view").HasConversion<string>();
        lambdas.Property(l => l.SecretSalt).HasColumnName("secret_salt");

        lambdas.HasIndex(l => l.PublicKey).IsUnique();
        lambdas.HasIndex(l => l.PrivateKey).IsUnique();
        lambdas.HasIndex(l => l.Domain).IsUnique().HasFilter("domain IS NOT NULL");

        var deployments = builder.Entity<DeploymentEntity>();

        deployments.ToTable("deployments");

        deployments.HasKey(d => d.Id);

        deployments.Property(d => d.Id).HasColumnName("id");
        deployments.Property(d => d.LambdaId).HasColumnName("lambda_id");
        deployments.Property(d => d.Version).HasColumnName("version");
        deployments.Property(d => d.Created).HasColumnName("created");
        deployments.Property(d => d.Specification).HasColumnName("specification");
        deployments.Property(d => d.Change).HasColumnName("change");
        deployments.Property(d => d.Origin).HasColumnName("origin");

        deployments.HasIndex(d => new { d.LambdaId, d.Version }).IsUnique();

        var events = builder.Entity<EventEntity>();

        events.ToTable("events");

        events.HasKey(e => e.Id);

        events.Property(e => e.Id).HasColumnName("id");
        events.Property(e => e.Kind).HasColumnName("kind");
        events.Property(e => e.LambdaId).HasColumnName("lambda_id");
        events.Property(e => e.PublicKey).HasColumnName("public_key");
        events.Property(e => e.Occurred).HasColumnName("occurred");

        events.HasIndex(e => e.Occurred);
        events.HasIndex(e => new { e.Kind, e.Occurred });

        var activations = builder.Entity<ActivationEntity>();

        activations.ToTable("activations");

        activations.HasKey(a => a.Id);

        activations.Property(a => a.Id).HasColumnName("id");
        activations.Property(a => a.LambdaId).HasColumnName("lambda_id");
        activations.Property(a => a.Version).HasColumnName("version");
        activations.Property(a => a.Started).HasColumnName("started");
        activations.Property(a => a.Origin).HasColumnName("origin");
        activations.Property(a => a.Ended).HasColumnName("ended");
        activations.Property(a => a.EndedBy).HasColumnName("ended_by");

        activations.HasIndex(a => new { a.LambdaId, a.Started });

        activations.HasOne(a => a.Lambda)
                   .WithMany()
                   .HasForeignKey(a => a.LambdaId)
                   .OnDelete(DeleteBehavior.Cascade);

        var showcases = builder.Entity<ShowcaseEntity>();

        showcases.ToTable("showcases");

        showcases.HasKey(s => s.LambdaId);

        showcases.Property(s => s.LambdaId).HasColumnName("lambda_id").ValueGeneratedNever();
        showcases.Property(s => s.Title).HasColumnName("title");
        showcases.Property(s => s.Description).HasColumnName("description");
        showcases.Property(s => s.Image).HasColumnName("image");
        showcases.Property(s => s.ImageType).HasColumnName("image_type");
        showcases.Property(s => s.Created).HasColumnName("created");
        showcases.Property(s => s.Updated).HasColumnName("updated");

        showcases.HasOne(s => s.Lambda)
                 .WithOne()
                 .HasForeignKey<ShowcaseEntity>(s => s.LambdaId)
                 .OnDelete(DeleteBehavior.Cascade);

        var sources = builder.Entity<SourceEntity>();

        sources.ToTable("sources");

        sources.HasKey(s => s.LambdaId);

        sources.Property(s => s.LambdaId).HasColumnName("lambda_id").ValueGeneratedNever();
        sources.Property(s => s.Published).HasColumnName("published");
        sources.Property(s => s.License).HasColumnName("license");
        sources.Property(s => s.Author).HasColumnName("author");
        sources.Property(s => s.Stars).HasColumnName("stars");
        sources.Property(s => s.About).HasColumnName("about");
        sources.Property(s => s.AboutVersion).HasColumnName("about_version");
        sources.Property(s => s.PublishedAt).HasColumnName("published_at");
        sources.Property(s => s.Updated).HasColumnName("updated");

        sources.HasOne(s => s.Lambda)
               .WithOne()
               .HasForeignKey<SourceEntity>(s => s.LambdaId)
               .OnDelete(DeleteBehavior.Cascade);

        var settings = builder.Entity<SettingEntity>();

        settings.ToTable("settings");

        settings.HasKey(s => s.Key);

        settings.Property(s => s.Key).HasColumnName("key");
        settings.Property(s => s.Value).HasColumnName("value");

        var stores = builder.Entity<DataStoreEntity>();

        stores.ToTable("data_stores");

        stores.HasKey(s => new { s.LambdaId, s.Kind });

        stores.Property(s => s.LambdaId).HasColumnName("lambda_id");
        stores.Property(s => s.Kind).HasColumnName("kind");
        stores.Property(s => s.Enabled).HasColumnName("enabled");
        stores.Property(s => s.Changed).HasColumnName("changed");

        stores.HasOne(s => s.Lambda)
              .WithMany()
              .HasForeignKey(s => s.LambdaId)
              .OnDelete(DeleteBehavior.Cascade);

        var features = builder.Entity<FeatureEntity>();

        features.ToTable("features");

        features.HasKey(f => f.Id);

        features.Property(f => f.Id).HasColumnName("id");
        features.Property(f => f.LambdaId).HasColumnName("lambda_id");
        features.Property(f => f.Key).HasColumnName("key");
        features.Property(f => f.Name).HasColumnName("name");
        features.Property(f => f.Branch).HasColumnName("branch");
        features.Property(f => f.Specification).HasColumnName("specification");
        features.Property(f => f.Change).HasColumnName("change");
        features.Property(f => f.BaseVersion).HasColumnName("base_version");
        features.Property(f => f.Origin).HasColumnName("origin");
        features.Property(f => f.Created).HasColumnName("created");
        features.Property(f => f.Modified).HasColumnName("modified");
        features.Property(f => f.Preview).HasColumnName("preview");
        features.Property(f => f.Previewed).HasColumnName("previewed");
        features.Property(f => f.Revision).HasColumnName("revision");
        features.Property(f => f.PreviewOf).HasColumnName("preview_of");

        features.HasIndex(f => f.Key).IsUnique();
        features.HasIndex(f => f.LambdaId);
        features.HasIndex(f => new { f.LambdaId, f.Branch }).IsUnique();

        features.HasOne(f => f.Lambda)
                .WithMany()
                .HasForeignKey(f => f.LambdaId)
                .OnDelete(DeleteBehavior.Cascade);

        var secrets = builder.Entity<SecretEntity>();

        secrets.ToTable("secrets");

        secrets.HasKey(s => s.Id);

        secrets.Property(s => s.Id).HasColumnName("id");
        secrets.Property(s => s.LambdaId).HasColumnName("lambda_id");
        secrets.Property(s => s.FeatureId).HasColumnName("feature_id");
        secrets.Property(s => s.Name).HasColumnName("name");
        secrets.Property(s => s.Value).HasColumnName("value");
        secrets.Property(s => s.Created).HasColumnName("created");
        secrets.Property(s => s.Changed).HasColumnName("changed");

        secrets.HasIndex(s => new { s.LambdaId, s.FeatureId, s.Name });

        secrets.HasOne<LambdaEntity>()
               .WithMany()
               .HasForeignKey(s => s.LambdaId)
               .OnDelete(DeleteBehavior.Cascade);

        secrets.HasOne<FeatureEntity>()
               .WithMany()
               .HasForeignKey(s => s.FeatureId)
               .OnDelete(DeleteBehavior.Cascade);

        deployments.HasOne(d => d.Lambda)
                   .WithMany(l => l.Deployments)
                   .HasForeignKey(d => d.LambdaId)
                   .OnDelete(DeleteBehavior.Cascade);
    }

}
