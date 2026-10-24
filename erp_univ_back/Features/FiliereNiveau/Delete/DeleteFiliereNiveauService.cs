namespace erp_univ_back.Features.FiliereNiveau.Delete;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IDeleteFiliereNiveauService
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class DeleteFiliereNiveauService(ErpUnivDbContext db) : IDeleteFiliereNiveauService
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await db.FiliereNiveaux.FirstOrDefaultAsync(fn => fn.Id == id, ct);
        if (entity is null)
        {
            throw new NotFoundException($"L'association filière-niveau avec l'identifiant '{id}' n'existe pas.");
        }

        db.FiliereNiveaux.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
