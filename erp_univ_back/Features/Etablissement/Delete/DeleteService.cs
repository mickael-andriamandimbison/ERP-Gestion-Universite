using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;

namespace erp_univ_back.Features.Etablissement.Delete;

public interface IDeleteService
{
    Task ExecuteAsync(Guid id);
}

public class DeleteService(ErpUnivDbContext db) : IDeleteService
{
    public async Task ExecuteAsync(Guid id)
    {
        var etablissement = await db.Etablissements.FindAsync(id);
        if (etablissement is null)
        {
            throw new NotFoundException("Etablissement non trouvé");
        }
        db.Etablissements.Remove(etablissement);
        await db.SaveChangesAsync();
    }
}
