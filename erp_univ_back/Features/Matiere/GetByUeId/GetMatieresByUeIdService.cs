namespace erp_univ_back.Features.Matiere.GetByUeId;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IGetMatieresByUeIdService
{
    Task<List<MatiereItemDto>> ExecuteAsync(GetMatieresByUeIdRequest request, CancellationToken ct = default);
}

public class GetMatieresByUeIdService(ErpUnivDbContext db) : IGetMatieresByUeIdService
{
    public async Task<List<MatiereItemDto>> ExecuteAsync(GetMatieresByUeIdRequest request, CancellationToken ct = default)
    {
        var ueExiste = await db.UniteEnseignements
            .AnyAsync(ue => ue.Id == request.UeId, ct);

        if (!ueExiste)
        {
            throw new NotFoundException($"L'unité d'enseignement avec l'identifiant '{request.UeId}' n'existe pas.");
        }

        var matieres = await db.Matieres
            .AsNoTracking()
            .Where(m => m.UniteEnseignementId == request.UeId)
            .OrderBy(m => m.Code)
            .Select(m => new MatiereItemDto(
                m.Id,
                m.Code,
                m.Nom,
                m.Coefficient
            ))
            .ToListAsync(ct);

        return matieres;
    }
}

