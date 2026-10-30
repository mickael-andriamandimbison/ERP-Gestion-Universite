namespace erp_univ_back.Features.Etablissement.Delete;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IDeleteEtablissementService
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class DeleteEtablissementService(ErpUnivDbContext db) : IDeleteEtablissementService
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var etablissement = await db.Etablissements.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException("Etablissement non trouvé");

        db.Etablissements.Remove(etablissement);
        await db.SaveChangesAsync(ct);
    }
}
