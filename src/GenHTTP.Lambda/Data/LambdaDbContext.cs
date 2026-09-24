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
        lambdas.Property(l => l.IsExample).HasColumnName("is_example");

        lambdas.HasIndex(l => l.PublicKey).IsUnique();
        lambdas.HasIndex(l => l.PrivateKey).IsUnique();

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

        deployments.HasOne(d => d.Lambda)
                   .WithMany(l => l.Deployments)
                   .HasForeignKey(d => d.LambdaId)
                   .OnDelete(DeleteBehavior.Cascade);
    }

}
