using erp_univ_back.Shared.Data;
using Mapster;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Shared.Exceptions;

namespace erp_univ_back.Features.Etablissement.Update;

public interface IUpdateService
{
    Task<EtablissementResponse> ExecuteAsync(Guid id, EtablissementRequest request);
}

public class UpdateService(ErpUnivDbContext db) : IUpdateService
{
    public async Task<EtablissementResponse> ExecuteAsync(Guid id, EtablissementRequest request)
    {
        var etablissement = await db.Etablissements.FindAsync(id)
            ?? throw new NotFoundException("Etablissement non trouvé");

        request.Adapt(etablissement);
        await db.SaveChangesAsync();

        return etablissement.Adapt<EtablissementResponse>();
    }
}