using erp_univ_back.Shared.Data;
using Mapster;
using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etablissement.Update
{
    public interface IUpdateService
    {
        Task<EtablissementResponse> ExecuteAsync(Guid id, EtablissementRequest request, CancellationToken ct = default);
    }

    public class UpdateService(ErpUnivDbContext db) : IUpdateService
    {
        public async Task<EtablissementResponse> ExecuteAsync(Guid id, EtablissementRequest request, CancellationToken ct = default)
        {
            var etablissement = await db.Etablissements.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new NotFoundException("Etablissement non trouvé");

            request.Adapt(etablissement);
            await db.SaveChangesAsync(ct);

            return etablissement.Adapt<EtablissementResponse>();
        }
    }
}
