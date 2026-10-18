using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Professeur.GetById;

public interface IGetByIdService
{
    Task<ProfesseurGetByIdResponse> ExecuteAsync(Guid id, CancellationToken ct);
}

public class GetByIdService(ErpUnivDbContext _db) : IGetByIdService
{
    public async Task<ProfesseurGetByIdResponse> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var professeur = await _db.Professeurs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (professeur is null)
            throw new NotFoundException("Professeur introuvable.");

        return professeur.Adapt<ProfesseurGetByIdResponse>();
    }
}
