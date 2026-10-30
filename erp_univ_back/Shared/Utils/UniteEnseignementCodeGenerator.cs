namespace erp_univ_back.Shared.Utils;

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

        // Numérotation dès le début sur au moins deux chiffres : 01, 02, etc.
        var compteur = 1;
        while (await _db.UniteEnseignements.AnyAsync(u => u.Code == $"{baseCode}-{compteur:D2}", ct))
        {
            compteur++;
        }

        return $"{baseCode}-{compteur:D2}";
    }
}
