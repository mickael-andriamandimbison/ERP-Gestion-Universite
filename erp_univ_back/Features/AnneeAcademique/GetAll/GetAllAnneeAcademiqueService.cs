namespace erp_univ_back.Features.AnneeAcademique.GetAll;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetAllAnneeAcademiqueService
{
    Task<List<GetAllAnneeAcademiqueResponse>> ExecuteAsync(GetAllAnneeAcademiqueRequest request, CancellationToken ct = default);
}

public class GetAllAnneeAcademiqueService(ErpUnivDbContext _db) : IGetAllAnneeAcademiqueService
{
    public async Task<List<GetAllAnneeAcademiqueResponse>> ExecuteAsync(GetAllAnneeAcademiqueRequest request, CancellationToken ct = default)
    {
        var query = _db.AnneeAcademiques.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var motif = QueryHelper.VersMotifRecherche(request.Recherche);
            query = query.Where(a => EF.Functions.ILike(a.Libelle, motif));
        }

        if (request.EstActive.HasValue)
        {
            query = query.Where(a => a.EstActive == request.EstActive.Value);
        }

        var entities = await query
            .OrderByDescending(a => a.EstActive)
            .ThenByDescending(a => a.DateDebut)
            .ToListAsync(ct);

        return entities.Adapt<List<GetAllAnneeAcademiqueResponse>>();
    }
}
