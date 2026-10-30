namespace erp_univ_back.Features.AnneeAcademique.GetActive;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetActiveAnneeAcademiqueService
{
    Task<GetActiveAnneeAcademiqueResponse> ExecuteAsync(CancellationToken ct = default);
}

public class GetActiveAnneeAcademiqueService(ErpUnivDbContext _db) : IGetActiveAnneeAcademiqueService
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
