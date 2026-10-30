namespace erp_univ_back.Features.UniteEnseignement.GetById;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetUniteEnseignementByIdService
{
    Task<GetUniteEnseignementByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class GetUniteEnseignementByIdService(ErpUnivDbContext db) : IGetUniteEnseignementByIdService
{
    public async Task<GetUniteEnseignementByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var ue = await db.UniteEnseignements
            .Include(u => u.FiliereNiveau)
                .ThenInclude(fn => fn.Filiere)
            .Include(u => u.FiliereNiveau)
                .ThenInclude(fn => fn.Niveau)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"L'unité d'enseignement avec l'identifiant '{id}' n'existe pas.");

        return ue.Adapt<GetUniteEnseignementByIdResponse>();
    }
}

