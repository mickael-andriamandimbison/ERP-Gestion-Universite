namespace erp_univ_back.Features.FiliereNiveau.GetAll;

using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;

public interface IGetAllFiliereNiveauService
{
    Task<List<FiliereNiveauDto>> ExecuteAsync(CancellationToken ct = default);
}

public class GetAllFiliereNiveauService(ErpUnivDbContext db) : IGetAllFiliereNiveauService
{
    public async Task<List<FiliereNiveauDto>> ExecuteAsync(CancellationToken ct = default)
    {
        return await db.FiliereNiveaux
            .AsNoTracking()
            .OrderBy(fn => fn.LibelleAffiche)
            .Select(fn => new FiliereNiveauDto(
                fn.Id,
                (fn.LibelleAffiche ?? string.Empty).Replace(" - ", "-")
            ))
            .ToListAsync(ct);
    }
}
