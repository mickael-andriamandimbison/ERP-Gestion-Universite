namespace erp_univ_back.Features.Matiere.Delete;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IDeleteMatiereService
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class DeleteMatiereService(ErpUnivDbContext db) : IDeleteMatiereService
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var matiere = await db.Matieres.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"La matière avec l'identifiant '{id}' n'existe pas.");

        db.Matieres.Remove(matiere);
        await db.SaveChangesAsync(ct);
    }
}

