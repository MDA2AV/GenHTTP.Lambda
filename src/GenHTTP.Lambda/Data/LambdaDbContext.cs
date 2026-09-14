using GenHTTP.Lambda.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace GenHTTP.Lambda.Data;

/// <summary>
/// Entity framework access to the SQLite file holding the lambda meta data.
/// The schema itself is maintained by Evolve, see <see cref="Migrator" />.
/// </summary>
public sealed class LambdaDbContext(DbContextOptions<LambdaDbContext> options) : DbContext(options)
{

    public DbSet<LambdaEntity> Lambdas => Set<LambdaEntity>();

    public DbSet<DeploymentEntity> Deployments => Set<DeploymentEntity>();

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

        deployments.HasIndex(d => new { d.LambdaId, d.Version }).IsUnique();

        deployments.HasOne(d => d.Lambda)
                   .WithMany(l => l.Deployments)
                   .HasForeignKey(d => d.LambdaId)
                   .OnDelete(DeleteBehavior.Cascade);
    }

}
