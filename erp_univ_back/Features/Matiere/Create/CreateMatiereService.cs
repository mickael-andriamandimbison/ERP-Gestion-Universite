using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Matiere.Create;

public interface ICreateMatiereService
{
    Task<CreateMatiereResponse> ExecuteAsync(CreateMatiereRequest request, CancellationToken ct);
}

public class CreateMatiereService(IValidator<CreateMatiereRequest> validator, ErpUnivDbContext db) : ICreateMatiereService
{
    public async Task<CreateMatiereResponse> ExecuteAsync(CreateMatiereRequest request, CancellationToken ct)
    {
        var validatorResult = await validator.ValidateAsync(request, ct);

        if (!validatorResult.IsValid)
        {
            throw new BadRequestException(validatorResult.Errors.First().ErrorMessage);
        }

        var uniteEnseignement = await db.UniteEnseignements
            .Include(x => x.FiliereNiveau).ThenInclude(f => f.Filiere)
            .Include(x => x.FiliereNiveau).ThenInclude(f => f.Niveau)
            .FirstOrDefaultAsync(x => x.Id == request.UniteEnseignementId, ct)
            ?? throw new NotFoundException($"L'unité d'enseignement '{request.UniteEnseignementId}' est introuvable.");

        // 1. Vérification si dans l'unité d'enseignement il existe déjà une matière avec le même nom (insensible à la casse)
        var nomNormalise = request.Nom.Trim().ToLowerInvariant();
        var matiereNomExiste = await db.Matieres.AnyAsync(
            m => m.UniteEnseignementId == request.UniteEnseignementId &&
                 m.Nom.ToLowerInvariant() == nomNormalise,
            ct);

        if (matiereNomExiste)
        {
            throw new ConflictException($"Une matière nommée '{request.Nom.Trim()}' existe déjà dans cette unité d'enseignement.");
        }

        // 2. Génération du code matière : CODE-FILIERE-NIVEAU
        var codePrefix = request.Code.Trim().ToUpperInvariant();
        var codeMatiere = $"{codePrefix}-{uniteEnseignement.FiliereNiveau.Filiere.Code.Trim()}-{uniteEnseignement.FiliereNiveau.Niveau.Code.Trim()}";

        // 3. Vérification de l'unicité du code de la matière
        var codeExiste = await db.Matieres.AnyAsync(
            m => m.Code.ToLowerInvariant() == codeMatiere.ToLowerInvariant(),
            ct);

        if (codeExiste)
        {
            throw new ConflictException($"Une matière avec le code '{codeMatiere}' existe déjà.");
        }

        var newMatiere = request.Adapt<MatiereEntity>();
        newMatiere.Nom = request.Nom.Trim();
        newMatiere.Code = codeMatiere;
        newMatiere.UniteEnseignement = uniteEnseignement;

        db.Matieres.Add(newMatiere);
        await db.SaveChangesAsync(ct);

        return newMatiere.Adapt<CreateMatiereResponse>();
    }
}
