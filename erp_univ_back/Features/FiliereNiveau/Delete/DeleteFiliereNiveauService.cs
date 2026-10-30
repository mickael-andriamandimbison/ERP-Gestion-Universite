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

        var aDesInscriptions = await db.Inscriptions.AnyAsync(i => i.FiliereNiveauId == id, ct);
        if (aDesInscriptions)
        {
            throw new ConflictException("Cette association filière-niveau ne peut pas être supprimée car des inscriptions y sont rattachées.");
        }

        db.FiliereNiveaux.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}
