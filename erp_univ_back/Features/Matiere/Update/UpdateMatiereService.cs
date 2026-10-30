namespace erp_univ_back.Features.Matiere.Update;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IUpdateMatiereService
{
    Task<UpdateMatiereResponse> ExecuteAsync(Guid id, UpdateMatiereRequest request, CancellationToken ct = default);
}

public class UpdateMatiereService(
    ErpUnivDbContext db,
    IValidator<UpdateMatiereRequest> validator) : IUpdateMatiereService
{
    public async Task<UpdateMatiereResponse> ExecuteAsync(Guid id, UpdateMatiereRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        var matiere = await db.Matieres
            .Include(m => m.UniteEnseignement)
                .ThenInclude(ue => ue.FiliereNiveau)
                    .ThenInclude(fn => fn.Filiere)
            .Include(m => m.UniteEnseignement)
                .ThenInclude(ue => ue.FiliereNiveau)
                    .ThenInclude(fn => fn.Niveau)
            .FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException($"La matière avec l'identifiant '{id}' n'existe pas.");

        // Si l'UE est modifiée
        if (request.UniteEnseignementId.HasValue && request.UniteEnseignementId.Value != matiere.UniteEnseignementId)
        {
            var newUe = await db.UniteEnseignements
                .Include(ue => ue.FiliereNiveau)
                    .ThenInclude(fn => fn.Filiere)
                .Include(ue => ue.FiliereNiveau)
                    .ThenInclude(fn => fn.Niveau)
                .FirstOrDefaultAsync(ue => ue.Id == request.UniteEnseignementId.Value, ct)
                ?? throw new NotFoundException($"L'unité d'enseignement avec l'identifiant '{request.UniteEnseignementId.Value}' n'existe pas.");

            matiere.UniteEnseignementId = newUe.Id;
            matiere.UniteEnseignement = newUe;
        }

        // 1. Unicité du nom au sein de la même UE (insensible à la casse)
        var nomNormalise = request.Nom.Trim().ToLowerInvariant();
        var nomExiste = await db.Matieres.AnyAsync(
            m => m.Id != id &&
                 m.UniteEnseignementId == matiere.UniteEnseignementId &&
                 m.Nom.ToLowerInvariant() == nomNormalise,
            ct);

        if (nomExiste)
        {
            throw new ConflictException($"Une matière nommée '{request.Nom.Trim()}' existe déjà dans cette unité d'enseignement.");
        }

        // 2. Gestion du code de la matière
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var codePrefix = request.Code.Trim().ToUpperInvariant();
            var ue = matiere.UniteEnseignement;
            var targetCode = codePrefix.Contains('-')
                ? codePrefix
                : $"{codePrefix}-{ue.FiliereNiveau.Filiere.Code.Trim()}-{ue.FiliereNiveau.Niveau.Code.Trim()}";

            var codeExiste = await db.Matieres.AnyAsync(
                m => m.Id != id &&
                     m.Code.ToLowerInvariant() == targetCode.ToLowerInvariant(),
                ct);

            if (codeExiste)
            {
                throw new ConflictException($"Une matière avec le code '{targetCode}' existe déjà.");
            }

            matiere.Code = targetCode;
        }

        matiere.Nom = request.Nom.Trim();
        matiere.Coefficient = request.Coefficient;

        await db.SaveChangesAsync(ct);

        return matiere.Adapt<UpdateMatiereResponse>();
    }
}

