using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etablissement.Delete
{
    public interface IDeleteService
    {
        Task ExecuteAsync(Guid id, CancellationToken ct = default);
    }

    public class DeleteService(ErpUnivDbContext db) : IDeleteService
    {
        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var etablissement = await db.Etablissements.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (etablissement is null)
            {
                throw new NotFoundException("Etablissement non trouvé");
            }
            db.Etablissements.Remove(etablissement);
            await db.SaveChangesAsync(ct);
        }
    }
}
