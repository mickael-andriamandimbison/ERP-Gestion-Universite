using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.Create
{
    public interface IMatriculeGenerator
    {
        Task<string> GenererAsync();
    }

    public class MatriculeGenerator(ErpUnivDbContext _db) : IMatriculeGenerator
    {
        public async Task<string> GenererAsync()
        {
            var annee = DateTime.UtcNow.Year;
            var count = await _db.Etudiants.CountAsync();
            return $"ETU-{annee}-{(count + 1):D5}";
        }
    }
}
