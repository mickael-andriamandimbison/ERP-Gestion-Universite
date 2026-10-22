namespace erp_univ_back.Features.FiliereNiveau.GetByFiliere;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetNiveauxByFiliereService
{
    Task<List<FiliereNiveauResponse>> ExecuteAsync(Guid filiereId, CancellationToken ct = default);
}

public class GetNiveauxByFiliereService(ErpUnivDbContext db) : IGetNiveauxByFiliereService
{
    public async Task<List<FiliereNiveauResponse>> ExecuteAsync(Guid filiereId, CancellationToken ct = default)
    {
        var filiereExiste = await db.Filieres.AnyAsync(f => f.Id == filiereId, ct);
        if (!filiereExiste)
        {
            throw new NotFoundException($"La filière avec l'identifiant '{filiereId}' n'existe pas.");
        }

        var filiereNiveaux = await db.FiliereNiveaux
            .AsNoTracking()
            .Where(fn => fn.FiliereId == filiereId)
            .OrderBy(fn => fn.Niveau.Ordre)
            .ProjectToType<FiliereNiveauResponse>()
            .ToListAsync(ct);

        return filiereNiveaux;
    }
}
