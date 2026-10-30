namespace erp_univ_back.Features.Matiere.GetById;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetMatiereByIdService
{
    Task<GetMatiereByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class GetMatiereByIdService(ErpUnivDbContext db) : IGetMatiereByIdService
{
    public async Task<GetMatiereByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var matiere = await db.Matieres
            .AsNoTracking()
            .Include(m => m.UniteEnseignement)
                .ThenInclude(ue => ue.FiliereNiveau)
                    .ThenInclude(fn => fn.Filiere)
            .Include(m => m.UniteEnseignement)
                .ThenInclude(ue => ue.FiliereNiveau)
                    .ThenInclude(fn => fn.Niveau)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"La matière avec l'identifiant '{id}' n'existe pas.");

        return matiere.Adapt<GetMatiereByIdResponse>();
    }
}

