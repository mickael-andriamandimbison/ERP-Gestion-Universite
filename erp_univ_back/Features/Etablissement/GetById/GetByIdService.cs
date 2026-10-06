namespace erp_univ_back.Features.Etablissement.GetById;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Features.Etablissement.Create;
using Mapster;

public interface IGetByIdService
{
    Task<EtablissementResponse> ExecuteAsync(Guid id);
}

public class GetByIdService(ErpUnivDbContext _db) : IGetByIdService
{
    public async Task<EtablissementResponse> ExecuteAsync(Guid id)
    {
        var etablissement = await _db.Etablissements.FindAsync(id);
        return etablissement?.Adapt<EtablissementResponse>() ?? throw new NotFoundException("Etablissement introuvable");
    }
}
