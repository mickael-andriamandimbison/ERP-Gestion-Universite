namespace erp_univ_back.Shared.Data;

using Microsoft.EntityFrameworkCore;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etudiant;
using erp_univ_back.Shared.Services;
using erp_univ_back.Features.Professeur;
using erp_univ_back.Features.AnneeAcademique;
using erp_univ_back.Shared.Entities;

public class ErpUnivDbContext(DbContextOptions<ErpUnivDbContext> options, ITenantService _tenantService) : DbContext(options)
{
    public DbSet<EtablissementEntity> Etablissements { get; set; } = null!;
    public DbSet<EtudiantEntity> Etudiants { get; set; } = null!;
    public DbSet<EtudiantParentEntity> EtudiantParents { get; set; } = null!;
    public DbSet<ParentEntity> Parents { get; set; } = null!;
    public DbSet<ProfesseurEntity> Professeurs { get; set; } = null!;
    public DbSet<AnneeAcademiqueEntity> AnneeAcademiques { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.Entity<EtablissementEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Nom).IsUnique();
                e.Property(x => x.Statut).HasConversion<string>().HasMaxLength(20);
            });

        mb.Entity<EtudiantEntity>(e =>
        {
            e.HasQueryFilter(e => e.EtablissementId == _tenantService.EtablissementId);
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Matricule)
            .IsUnique();
            e.HasIndex(x => x.NumeroCin)
            .IsUnique();
            e.HasIndex(x => x.NumeroTelphone)
            .IsUnique();
            e.Property(x => x.NumeroTelphone)
            .HasConversion<string>()
            .HasMaxLength(20);
            e.HasOne(x => x.Etablissement)
            .WithMany(x => x.Etudiants)
            .HasForeignKey(x => x.EtablissementId)
            .OnDelete(DeleteBehavior.Restrict);

        });

        mb.Entity<ProfesseurEntity>(e =>
        {
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);
            e.HasIndex(x => x.Matricule).IsUnique();
            e.HasIndex(x => x.NumeroCin)
            .IsUnique();
            e.HasIndex(x => x.NumeroTelephone)
            .IsUnique();
            e.Property(x => x.NumeroTelephone)
            .HasConversion<string>()
            .HasMaxLength(20);
            e.HasOne(x => x.Etablissement)
            .WithMany(x => x.Professeurs)
            .HasForeignKey(x => x.EtablissementId)
            .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<ParentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.NumeroTelphone)
            .IsUnique();
            e.Property(x => x.NumeroTelphone)
            .HasMaxLength(20);

        });

        mb.Entity<EtudiantParentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Etudiant)
            .WithMany(x => x.EtudiantParents)
            .HasForeignKey(x => x.EtudiantId)
            .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Parent)
            .WithMany(x => x.EtudiantParents)
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(10);
        });

        mb.Entity<AnneeAcademiqueEntity>(e =>
        {
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);
            e.HasOne(x => x.Etablissement)
            .WithMany(x => x.AnneeAcademiques)
            .HasForeignKey(x => x.EtablissementId)
            .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.EtablissementId, x.Libelle }).IsUnique();
            e.Property(x=>x.Libelle)
            .HasMaxLength(15)
            .IsRequired();

            e.Property(x=>x.DateDebut)
            .IsRequired();

            e.Property(x=>x.DateFin)
            .IsRequired();

            e.Property(x=>x.EstActive)
            .IsRequired();

        });
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DateCreation = DateTime.UtcNow;
                entry.Entity.DateModification = DateTime.UtcNow;

                var hasEtablissementId = entry.Properties.Any(p => p.Metadata.Name == "EtablissementId");

                if (hasEtablissementId && _tenantService.EtablissementId.HasValue)
                {
                    entry.Property("EtablissementId").CurrentValue = _tenantService.EtablissementId.Value;
                }
            }
            else
            {
                entry.Entity.DateModification = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}