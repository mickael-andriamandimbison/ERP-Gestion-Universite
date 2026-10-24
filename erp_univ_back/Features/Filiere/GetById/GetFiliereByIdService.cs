using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.GetById;

public interface IGetFiliereByIdService
{
    Task<GetFiliereByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class GetFiliereByIdService(ErpUnivDbContext _db) : IGetFiliereByIdService
{
    public async Task<GetFiliereByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var filiere = await _db.Filieres
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new NotFoundException($"Filière avec l'identifiant '{id}' introuvable.");

        return filiere.Adapt<GetFiliereByIdResponse>();
    }
}
