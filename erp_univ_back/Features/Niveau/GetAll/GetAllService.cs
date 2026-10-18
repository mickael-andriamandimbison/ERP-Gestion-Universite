namespace erp_univ_back.Features.Niveau.GetAll;

using erp_univ_back.Shared.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetAllNiveauxService
{
    Task<List<NiveauResponse>> ExecuteAsync(CancellationToken ct = default);
}

public class GetAllNiveauxService(ErpUnivDbContext db) : IGetAllNiveauxService
{
    public async Task<List<NiveauResponse>> ExecuteAsync(CancellationToken ct = default)
    {
        return await db.Niveaux
            .AsNoTracking()
            .OrderBy(n => n.Ordre)
            .ProjectToType<NiveauResponse>()
            .ToListAsync(ct);
    }
}
