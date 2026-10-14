
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.GetById
{
    public interface IGetByIdService
    {
        Task<EtudiantEntity> ExecuteAsync(Guid id, CancellationToken ct);
    }

    public class GetByIdService(ErpUnivDbContext _db) : IGetByIdService
    {
        public async Task<EtudiantEntity> ExecuteAsync(Guid id, CancellationToken ct)
        {
            var etudiant = await _db.Etudiants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return etudiant ?? throw new NotFoundException("Etudiant introuvable.");
        }
    }
}