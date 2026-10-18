namespace erp_univ_back.Features.Niveau.GetById;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetNiveauByIdService
{
    Task<NiveauDetailResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class GetNiveauByIdService(ErpUnivDbContext db) : IGetNiveauByIdService
{
    public async Task<NiveauDetailResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var niveau = await db.Niveaux
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == id, ct);

        if (niveau is null)
        {
            throw new NotFoundException($"Le niveau avec l'identifiant '{id}' n'existe pas.");
        }

        return niveau.Adapt<NiveauDetailResponse>();
    }
}
