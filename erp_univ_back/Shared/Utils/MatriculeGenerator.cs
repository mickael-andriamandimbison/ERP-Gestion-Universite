using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Shared.Utils
{
    public interface IMatriculeGenerator
    {
        Task<string> GenererAsync(bool isEtu);
    }

    public class MatriculeGenerator(ErpUnivDbContext _db) : IMatriculeGenerator
    {
        public async Task<string> GenererAsync(bool isEtu)
        {
            var annee = DateTime.UtcNow.Year;
            var count = isEtu ? await _db.Etudiants.CountAsync() : await _db.Professeurs.CountAsync();
            return $"{(isEtu ? "ETU" : "PROF")}-{annee}-{(count + 1):D5}";
        }
    }
}
