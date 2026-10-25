using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Shared.Utils
{
    public interface IMatriculeGenerator
    {
        Task<string> GenererAsync(bool isEtu, CancellationToken ct = default);
    }

    public class MatriculeGenerator(ErpUnivDbContext _db) : IMatriculeGenerator
    {
        public async Task<string> GenererAsync(bool isEtu, CancellationToken ct = default)
        {
            var annee = DateTime.UtcNow.Year;
            var count = isEtu ? await _db.Etudiants.CountAsync(ct) : await _db.Professeurs.CountAsync(ct);
            return $"{(isEtu ? "ETU" : "PROF")}-{annee}-{(count + 1):D5}";
        }
    }
}
