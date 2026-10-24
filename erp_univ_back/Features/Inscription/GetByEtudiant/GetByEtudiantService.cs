namespace erp_univ_back.Features.Inscription.GetByEtudiant;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetInscriptionsByEtudiantService
{
    Task<List<InscriptionByEtudiantResponse>> ExecuteAsync(Guid etudiantId, CancellationToken ct = default);
}

public class GetInscriptionsByEtudiantService(ErpUnivDbContext db) : IGetInscriptionsByEtudiantService
{
    public async Task<List<InscriptionByEtudiantResponse>> ExecuteAsync(Guid etudiantId, CancellationToken ct = default)
    {
        var etudiantExiste = await db.Etudiants.AnyAsync(e => e.Id == etudiantId, ct);
        if (!etudiantExiste)
        {
            throw new NotFoundException($"L'étudiant avec l'identifiant '{etudiantId}' n'existe pas.");
        }

        var historique = await db.Inscriptions
            .AsNoTracking()
            .Where(i => i.EtudiantId == etudiantId)
            .OrderByDescending(i => i.AnneeAcademique.DateDebut)
            .ThenByDescending(i => i.DateCreation)
            .ProjectToType<InscriptionByEtudiantResponse>()
            .ToListAsync(ct);

        return historique;
    }
}
