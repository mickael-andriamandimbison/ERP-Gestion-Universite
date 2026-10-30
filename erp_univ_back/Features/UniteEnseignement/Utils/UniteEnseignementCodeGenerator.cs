namespace erp_univ_back.Features.UniteEnseignement.Utils;

using erp_univ_back.Shared.Data;
using Microsoft.EntityFrameworkCore;

public interface IUniteEnseignementCodeGenerator
{
    Task<string> GenererAsync(Guid filiereNiveauId, CancellationToken ct = default);
    string FormaterBase(string filiereCode, string niveauCode);
}

public class UniteEnseignementCodeGenerator(ErpUnivDbContext _db) : IUniteEnseignementCodeGenerator
{
    public string FormaterBase(string filiereCode, string niveauCode)
    {
        return $"UE-{filiereCode.Trim().ToUpper()}-{niveauCode.Trim().ToUpper()}";
    }

    public async Task<string> GenererAsync(Guid filiereNiveauId, CancellationToken ct = default)
    {
        var filiereNiveau = await _db.FiliereNiveaux
            .Include(fn => fn.Filiere)
            .Include(fn => fn.Niveau)
            .FirstOrDefaultAsync(fn => fn.Id == filiereNiveauId, ct)
            ?? throw new InvalidOperationException($"FiliereNiveau avec l'identifiant {filiereNiveauId} introuvable.");

        var baseCode = FormaterBase(filiereNiveau.Filiere.Code, filiereNiveau.Niveau.Code);

        // Si le code de base n'existe pas encore dans cet établissement, on l'utilise directement (ex: UE-INF-L1)
        var existeDeja = await _db.UniteEnseignements.AnyAsync(u => u.Code == baseCode, ct);
        if (!existeDeja)
        {
            return baseCode;
        }

        // Sinon, on incrémente un suffixe numérique pour garantir l'unicité (ex: UE-INF-L1-2, UE-INF-L1-3, ...)
        var compteur = 2;
        while (await _db.UniteEnseignements.AnyAsync(u => u.Code == $"{baseCode}-{compteur}", ct))
        {
            compteur++;
        }

        return $"{baseCode}-{compteur}";
    }
}

