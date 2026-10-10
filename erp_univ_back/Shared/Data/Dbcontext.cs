namespace erp_univ_back.Shared.Data;

using Microsoft.EntityFrameworkCore;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Shared.Entities;
using erp_univ_back.Features.Etudiant;

public class ErpUnivDbContext(DbContextOptions<ErpUnivDbContext> options) : DbContext(options)
{

    public DbSet<EtablissementEntity> Etablissements { get; set; }
    public DbSet<EtudiantEntity> Etudiants { get; set; }
    public DbSet<EtudiantParentEntity> EtudiantParents { get; set; }
    public DbSet<ParentEntity> Parents { get; set; }

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
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Matricule).IsUnique();
            e.HasIndex(x => x.NumeroCin).IsUnique();
            e.HasIndex(x => x.NumeroTelphone).IsUnique();
            e.Property(x => x.NumeroTelphone).HasConversion<string>().HasMaxLength(20);

        });

        mb.Entity<ParentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.NumeroTelphone).IsUnique();
            e.Property(x => x.NumeroTelphone).HasMaxLength(20);

        });

        mb.Entity<EtudiantParentEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Etudiant).WithMany(x => x.EtudiantParents).HasForeignKey(x => x.EtudiantId);
            e.HasOne(x => x.Parent).WithMany(x => x.EtudiantParents).HasForeignKey(x => x.ParentId);
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(10);
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
            }
            else
            {
                entry.Entity.DateModification = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}