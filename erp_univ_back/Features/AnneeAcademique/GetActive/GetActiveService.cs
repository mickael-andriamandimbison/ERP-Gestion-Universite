using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.AnneeAcademique.GetActive
{
    public interface IGetActiveService
    {
        Task<GetActiveAnneeAcademiqueResponse> ExecuteAsync(CancellationToken ct = default);
    }

    public class GetActiveService(ErpUnivDbContext _db) : IGetActiveService
    {
        public async Task<GetActiveAnneeAcademiqueResponse> ExecuteAsync(CancellationToken ct = default)
        {
            var activeEntity = await _db.AnneeAcademiques
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.EstActive, ct);

            if (activeEntity is null)
                throw new NotFoundException("Aucune année académique active n'a été trouvée pour cet établissement.");

            return activeEntity.Adapt<GetActiveAnneeAcademiqueResponse>();
        }
    }
}
