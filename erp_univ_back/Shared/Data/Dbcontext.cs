namespace erp_univ_back.Shared.Data;

using Microsoft.EntityFrameworkCore;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Features.Etudiant;
using erp_univ_back.Shared.Services;
using erp_univ_back.Features.Professeur;
using erp_univ_back.Features.AnneeAcademique;
using erp_univ_back.Features.Filiere;
using erp_univ_back.Features.Niveau;
using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Features.Inscription;
using erp_univ_back.Shared.Entities;

public class ErpUnivDbContext(DbContextOptions<ErpUnivDbContext> options, ITenantService _tenantService) : DbContext(options)
{
    public DbSet<EtablissementEntity> Etablissements { get; set; } = null!;
    public DbSet<EtudiantEntity> Etudiants { get; set; } = null!;
    public DbSet<EtudiantParentEntity> EtudiantParents { get; set; } = null!;
    public DbSet<ParentEntity> Parents { get; set; } = null!;
    public DbSet<ProfesseurEntity> Professeurs { get; set; } = null!;
    public DbSet<AnneeAcademiqueEntity> AnneeAcademiques { get; set; } = null!;
    public DbSet<FiliereEntity> Filieres { get; set; } = null!;
    public DbSet<NiveauEntity> Niveaux { get; set; } = null!;
    public DbSet<FiliereNiveauEntity> FiliereNiveaux { get; set; } = null!;
    public DbSet<InscriptionEntity> Inscriptions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // --- 1. Établissement (Entité racine globale, jamais filtrée par tenant) ---
        mb.Entity<EtablissementEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Nom).IsUnique();
            e.Property(x => x.Statut).HasConversion<string>().HasMaxLength(20);
        });

        // --- 2. Étudiant ---
        mb.Entity<EtudiantEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.HasIndex(x => new { x.EtablissementId, x.Matricule }).IsUnique();
            e.HasIndex(x => new { x.EtablissementId, x.NumeroCin }).IsUnique();
            e.HasIndex(x => new { x.EtablissementId, x.NumeroTelphone }).IsUnique();

            e.Property(x => x.NumeroTelphone)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.Etudiants)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 3. Professeur ---
        mb.Entity<ProfesseurEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.HasIndex(x => new { x.EtablissementId, x.Matricule }).IsUnique();
            e.HasIndex(x => new { x.EtablissementId, x.NumeroCin }).IsUnique();
            e.HasIndex(x => new { x.EtablissementId, x.NumeroTelephone }).IsUnique();

            e.Property(x => x.NumeroTelephone)
                .HasConversion<string>()
                .HasMaxLength(20);

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.Professeurs)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 4. Parent ---
        mb.Entity<ParentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.HasIndex(x => new { x.EtablissementId, x.NumeroTelphone }).IsUnique();
            e.Property(x => x.NumeroTelphone).HasMaxLength(20);

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.Parents)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 5. Table de jonction Étudiant <-> Parent ---
        mb.Entity<EtudiantParentEntity>(e =>
        {
            e.HasKey(x => x.Id);

            // Empêche d'associer deux fois le même parent au même étudiant
            e.HasIndex(x => new { x.EtudiantId, x.ParentId }).IsUnique();

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

        // --- 6. Année Académique ---
        mb.Entity<AnneeAcademiqueEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.HasIndex(x => new { x.EtablissementId, x.Libelle }).IsUnique();

            e.Property(x => x.Libelle).HasMaxLength(15).IsRequired();
            e.Property(x => x.DateDebut).IsRequired();
            e.Property(x => x.DateFin).IsRequired();
            e.Property(x => x.EstActive).IsRequired();

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.AnneeAcademiques)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 7. Filière ---
        mb.Entity<FiliereEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Nom).HasMaxLength(150).IsRequired();

            e.HasIndex(x => new { x.EtablissementId, x.Code }).IsUnique();

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.Filieres)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 8. Niveau ---
        mb.Entity<NiveauEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.Property(x => x.Code).HasMaxLength(10).IsRequired();
            e.Property(x => x.Nom).HasMaxLength(100).IsRequired();
            e.Property(x => x.Ordre).IsRequired();

            e.HasIndex(x => new { x.EtablissementId, x.Code }).IsUnique();

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.Niveaux)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 9. FilièreNiveau (Jonction académique) ---
        mb.Entity<FiliereNiveauEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.Property(x => x.LibelleAffiche).HasMaxLength(150);

            e.HasIndex(x => new { x.EtablissementId, x.FiliereId, x.NiveauId }).IsUnique();

            e.HasOne(x => x.Filiere)
                .WithMany(x => x.FiliereNiveaux)
                .HasForeignKey(x => x.FiliereId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Niveau)
                .WithMany(x => x.FiliereNiveaux)
                .HasForeignKey(x => x.NiveauId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.FiliereNiveaux)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // --- 10. Inscription ---
        mb.Entity<InscriptionEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasQueryFilter(x => x.EtablissementId == _tenantService.EtablissementId);

            e.Property(x => x.Statut)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Index unique composé anti-doublon (EtablissementId, EtudiantId, FiliereNiveauId, AnneeAcademiqueId)
            e.HasIndex(x => new { x.EtablissementId, x.EtudiantId, x.FiliereNiveauId, x.AnneeAcademiqueId }).IsUnique();

            e.HasOne(x => x.Etudiant)
                .WithMany(x => x.Inscriptions)
                .HasForeignKey(x => x.EtudiantId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.FiliereNiveau)
                .WithMany(x => x.Inscriptions)
                .HasForeignKey(x => x.FiliereNiveauId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.AnneeAcademique)
                .WithMany(x => x.Inscriptions)
                .HasForeignKey(x => x.AnneeAcademiqueId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Etablissement)
                .WithMany(x => x.Inscriptions)
                .HasForeignKey(x => x.EtablissementId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.DateCreation = DateTime.UtcNow;
                entry.Entity.DateModification = DateTime.UtcNow;

                var hasEtablissementId = entry.Properties.Any(p => p.Metadata.Name == "EtablissementId");

                // Injecte automatiquement l'EtablissementId courant ou lève 401 si manquant
                if (hasEtablissementId)
                {
                    entry.Property("EtablissementId").CurrentValue = _tenantService.ObtenirEtablissementId();
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