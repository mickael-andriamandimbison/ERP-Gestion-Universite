using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Features.Etablissement.Create;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etablissement.GetById
{
    public interface IGetByIdService
    {
        Task<EtablissementResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
    }

    public class GetByIdService(ErpUnivDbContext _db) : IGetByIdService
    {
        public async Task<EtablissementResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var etablissement = await _db.Etablissements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return etablissement?.Adapt<EtablissementResponse>() ?? throw new NotFoundException("Etablissement introuvable");
        }
    }
}
