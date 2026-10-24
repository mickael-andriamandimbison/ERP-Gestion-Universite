namespace erp_univ_back.Features.Etablissement.Update;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IUpdateEtablissementService
{
    Task<UpdateEtablissementResponse> ExecuteAsync(Guid id, UpdateEtablissementRequest request, CancellationToken ct = default);
}

public class UpdateEtablissementService(ErpUnivDbContext db) : IUpdateEtablissementService
{
    public async Task<UpdateEtablissementResponse> ExecuteAsync(Guid id, UpdateEtablissementRequest request, CancellationToken ct = default)
    {
        var etablissement = await db.Etablissements.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Etablissement non trouvé");

        request.Adapt(etablissement);
        await db.SaveChangesAsync(ct);

        return etablissement.Adapt<UpdateEtablissementResponse>();
    }
}
