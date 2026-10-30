namespace erp_univ_back.Features.UniteEnseignement.GetByFiliereNiveau;

using erp_univ_back.Features.AnneeAcademique;
using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface IGetUeByFiliereNiveauService
{
    Task<GetUeByFiliereNiveauResponse> ExecuteAsync(GetUeByFiliereNiveauRequest request, CancellationToken ct = default);
}

public class GetUeByFiliereNiveauService(ErpUnivDbContext db) : IGetUeByFiliereNiveauService
{
    public async Task<GetUeByFiliereNiveauResponse> ExecuteAsync(GetUeByFiliereNiveauRequest request, CancellationToken ct = default)
    {
        // 1. Récupération de l'année académique active (ou la plus récente)
        var anneeAcademique = await db.AnneeAcademiques
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.EstActive, ct)
            ?? await db.AnneeAcademiques
                .AsNoTracking()
                .OrderByDescending(a => a.DateDebut)
                .FirstOrDefaultAsync(ct);

        string? anneeAcademiqueLibelle = anneeAcademique?.Libelle;

        // 2. Résolution de la filière-niveau
        FiliereNiveauEntity? filiereNiveau = null;

        if (request.FiliereId.HasValue && request.NiveauId.HasValue)
        {
            // Filtre par filière et niveau dans la table filiere-niveaux
            filiereNiveau = await db.FiliereNiveaux
                .Include(fn => fn.Filiere)
                .Include(fn => fn.Niveau)
                .FirstOrDefaultAsync(fn => fn.FiliereId == request.FiliereId.Value && fn.NiveauId == request.NiveauId.Value, ct)
                ?? throw new NotFoundException($"Aucune association Filière-Niveau trouvée pour la filière '{request.FiliereId.Value}' et le niveau '{request.NiveauId.Value}'.");
        }
        else
        {
            // Valeur par défaut de sécurité : premier filiere-niveau trouvé
            filiereNiveau = await db.FiliereNiveaux
                .Include(fn => fn.Filiere)
                .Include(fn => fn.Niveau)
                .OrderBy(fn => fn.Filiere.Nom)
                .ThenBy(fn => fn.Niveau.Ordre)
                .FirstOrDefaultAsync(ct);
        }

        if (filiereNiveau is null)
        {
            return new GetUeByFiliereNiveauResponse(
                AnneAcademique: anneeAcademiqueLibelle,
                FiliereNom: null,
                FiliereId: null,
                NiveauNom: null,
                NiveauId: null,
                FiliereNiveauId: null,
                Semestres: []
            );
        }

        // 3. Récupération des UEs associées
        var ues = await db.UniteEnseignements
            .AsNoTracking()
            .Where(u => u.FiliereNiveauId == filiereNiveau.Id)
            .OrderBy(u => u.Semestre)
            .ThenBy(u => u.Code)
            .ToListAsync(ct);

        // 4. Regroupement par semestre avec la liste des UEs associées
        var semestres = ues
            .GroupBy(u => u.Semestre)
            .OrderBy(g => g.Key)
            .Select(g => new SemestreUeDto(
                Semestre: g.Key,
                TotalCredits: g.Sum(u => u.Credit),
                UnitesEnseignement: g.Select(u => new UniteEnseignementItemDto(
                    u.Id,
                    u.Code,
                    u.Nom,
                    u.Credit,
                    u.Semestre,
                    u.DateCreation
                )).ToList()
            ))
            .ToList();

        return new GetUeByFiliereNiveauResponse(
            AnneAcademique: anneeAcademiqueLibelle,
            FiliereNom: filiereNiveau.Filiere.Nom,
            FiliereId: filiereNiveau.FiliereId,
            NiveauNom: filiereNiveau.Niveau.Nom,
            NiveauId: filiereNiveau.NiveauId,
            FiliereNiveauId: filiereNiveau.Id,
            Semestres: semestres
        );
    }
}
