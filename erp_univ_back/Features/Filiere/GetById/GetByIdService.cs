using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.GetById
{
    public interface IGetByIdService
    {
        Task<FiliereGetByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
    }

    public class GetByIdService(ErpUnivDbContext _db) : IGetByIdService
    {
        public async Task<FiliereGetByIdResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var filiere = await _db.Filieres
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id, ct)
                ?? throw new NotFoundException($"Filière avec l'identifiant '{id}' introuvable.");

            return filiere.Adapt<FiliereGetByIdResponse>();
        }
    }
}
