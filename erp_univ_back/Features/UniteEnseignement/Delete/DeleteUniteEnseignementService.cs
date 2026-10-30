namespace erp_univ_back.Features.UniteEnseignement.Delete;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IDeleteUniteEnseignementService
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class DeleteUniteEnseignementService(ErpUnivDbContext db) : IDeleteUniteEnseignementService
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var ue = await db.UniteEnseignements.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (ue is null)
        {
            throw new NotFoundException($"L'unité d'enseignement avec l'identifiant '{id}' n'existe pas.");
        }

        db.UniteEnseignements.Remove(ue);
        await db.SaveChangesAsync(ct);
    }
}

