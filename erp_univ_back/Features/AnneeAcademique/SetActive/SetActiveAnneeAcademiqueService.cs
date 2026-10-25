namespace erp_univ_back.Features.AnneeAcademique.SetActive;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface ISetActiveAnneeAcademiqueService
{
    Task<SetActiveAnneeAcademiqueResponse> ExecuteAsync(Guid id, CancellationToken ct = default);
}

public class SetActiveAnneeAcademiqueService(ErpUnivDbContext _db) : ISetActiveAnneeAcademiqueService
{
    public async Task<SetActiveAnneeAcademiqueResponse> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var target = await _db.AnneeAcademiques.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Année académique introuvable.");

        // Désactiver toutes les autres années actives pour cet établissement
        var currentActives = await _db.AnneeAcademiques
            .Where(a => a.EstActive && a.Id != id)
            .ToListAsync(ct);

        foreach (var annee in currentActives)
        {
            annee.EstActive = false;
            annee.DateModification = DateTime.UtcNow;
        }

        target.EstActive = true;
        target.DateModification = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new SetActiveAnneeAcademiqueResponse(
            target.Id,
            target.Libelle,
            target.DateDebut,
            target.DateFin,
            target.EstActive,
            $"L'année académique '{target.Libelle}' est désormais active."
        );
    }
}
