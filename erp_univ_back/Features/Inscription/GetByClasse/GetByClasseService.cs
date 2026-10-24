namespace erp_univ_back.Features.Inscription.GetByClasse;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetInscriptionsByClasseService
{
    Task<List<InscriptionByClasseResponse>> ExecuteAsync(
        Guid filiereNiveauId,
        Guid anneeAcademiqueId,
        CancellationToken ct = default);
}

public class GetInscriptionsByClasseService(ErpUnivDbContext db) : IGetInscriptionsByClasseService
{
    public async Task<List<InscriptionByClasseResponse>> ExecuteAsync(
        Guid filiereNiveauId,
        Guid anneeAcademiqueId,
        CancellationToken ct = default)
    {
        var classeExiste = await db.FiliereNiveaux.AnyAsync(fn => fn.Id == filiereNiveauId, ct);
        if (!classeExiste)
        {
            throw new NotFoundException($"La classe (filière-niveau) avec l'identifiant '{filiereNiveauId}' n'existe pas.");
        }

        var anneeExiste = await db.AnneeAcademiques.AnyAsync(a => a.Id == anneeAcademiqueId, ct);
        if (!anneeExiste)
        {
            throw new NotFoundException($"L'année académique avec l'identifiant '{anneeAcademiqueId}' n'existe pas.");
        }

        var listeAppel = await db.Inscriptions
            .AsNoTracking()
            .Where(i => i.FiliereNiveauId == filiereNiveauId && i.AnneeAcademiqueId == anneeAcademiqueId)
            .OrderBy(i => i.Etudiant.Nom)
            .ThenBy(i => i.Etudiant.Prenom)
            .ProjectToType<InscriptionByClasseResponse>()
            .ToListAsync(ct);

        return listeAppel;
    }
}
