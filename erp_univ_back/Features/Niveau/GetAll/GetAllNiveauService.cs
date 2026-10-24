namespace erp_univ_back.Features.Niveau.GetAll;

using erp_univ_back.Shared.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetAllNiveauService
{
    Task<List<GetAllNiveauResponse>> ExecuteAsync(CancellationToken ct = default);
}

public class GetAllNiveauService(ErpUnivDbContext db) : IGetAllNiveauService
{
    public async Task<List<GetAllNiveauResponse>> ExecuteAsync(CancellationToken ct = default)
    {
        return await db.Niveaux
            .AsNoTracking()
            .OrderBy(n => n.Ordre)
            .ProjectToType<GetAllNiveauResponse>()
            .ToListAsync(ct);
    }
}
