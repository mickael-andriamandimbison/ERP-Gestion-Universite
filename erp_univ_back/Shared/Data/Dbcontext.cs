namespace erp_univ_back.Shared.Data;

using Microsoft.EntityFrameworkCore;
using erp_univ_back.Features.Etablissement;
using erp_univ_back.Shared.Entities;

public class ErpUnivDbContext(DbContextOptions<ErpUnivDbContext> options) : DbContext(options)
{

    public DbSet<EtablissementEntity> Etablissements { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);
        mb.Entity<EtablissementEntity>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.Nom).IsUnique();
                e.Property(x => x.Statut).HasConversion<string>().HasMaxLength(20);
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