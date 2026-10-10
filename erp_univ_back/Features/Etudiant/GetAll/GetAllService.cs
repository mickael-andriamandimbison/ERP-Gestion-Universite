

using erp_univ_back.Shared.Data;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.GetAll
{
    public interface IGetAllEtudiantService
    {
        Task<IEnumerable<EtudiantGetAllResponse>> ExecuteAsync(CancellationToken ct);
    }

    public class GetAllEtudiantService(ErpUnivDbContext _db) : IGetAllEtudiantService
    {

        public async Task<IEnumerable<EtudiantGetAllResponse>> ExecuteAsync(CancellationToken ct)
        {
            var etudiants = await _db.Etudiants.ToListAsync(ct);
            return etudiants.Adapt<IEnumerable<EtudiantGetAllResponse>>();
        }
    }
}