using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Professeur.GetById;

public interface IGetProfesseurByIdService
{
    Task<GetProfesseurByIdResponse> ExecuteAsync(Guid id, CancellationToken ct);
}

public interface IGetByIdService : IGetProfesseurByIdService
{
}

public class GetProfesseurByIdService(ErpUnivDbContext _db) : IGetProfesseurByIdService, IGetByIdService
{
    public async Task<GetProfesseurByIdResponse> ExecuteAsync(Guid id, CancellationToken ct)
    {
        var professeur = await _db.Professeurs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (professeur is null)
            throw new NotFoundException("Professeur introuvable.");

        return professeur.Adapt<GetProfesseurByIdResponse>();
    }
}
