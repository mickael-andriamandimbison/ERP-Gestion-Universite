namespace erp_univ_back.Features.Etablissement.GetById;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetEtablissementByIdService
{
    Task<GetEtablissementByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class GetEtablissementByIdService(ErpUnivDbContext _db) : IGetEtablissementByIdService
{
    public async Task<GetEtablissementByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var etablissement = await _db.Etablissements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return etablissement?.Adapt<GetEtablissementByIdResponse>() ?? throw new NotFoundException("Etablissement introuvable");
    }
}
