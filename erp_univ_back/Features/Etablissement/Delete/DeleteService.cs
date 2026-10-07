using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;

namespace erp_univ_back.Features.Etablissement.Delete;

public interface IDeleteService
{
    Task ExecuteAsync(Guid id, CancellationToken ct);
}

public class DeleteService(ErpUnivDbContext db) : IDeleteService
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct)
    {
        var etablissement = await db.Etablissements.FindAsync(id);
        if (etablissement is null)
        {
            throw new NotFoundException("Etablissement non trouvé");
        }
        db.Etablissements.Remove(etablissement);
        await db.SaveChangesAsync(ct);
    }
}
