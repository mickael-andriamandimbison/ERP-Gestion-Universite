namespace erp_univ_back.Features.Niveau.Delete;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IDeleteNiveauService
{
    Task ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class DeleteNiveauService(ErpUnivDbContext db) : IDeleteNiveauService
{
    public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var niveau = await db.Niveaux.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (niveau is null)
        {
            throw new NotFoundException($"Le niveau avec l'identifiant '{id}' n'existe pas.");
        }

        var estAssocie = await db.FiliereNiveaux.AnyAsync(fn => fn.NiveauId == id, ct);
        if (estAssocie)
        {
            throw new ConflictException("Ce niveau ne peut pas être supprimé car il est déjà associé à une ou plusieurs filières.");
        }

        db.Niveaux.Remove(niveau);
        await db.SaveChangesAsync(ct);
    }
}
