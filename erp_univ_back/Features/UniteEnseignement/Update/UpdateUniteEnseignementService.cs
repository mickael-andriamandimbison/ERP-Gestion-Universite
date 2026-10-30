namespace erp_univ_back.Features.UniteEnseignement.Update;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IUpdateUniteEnseignementService
{
    Task<UpdateUniteEnseignementResponse> ExecuteAsync(Guid id, UpdateUniteEnseignementRequest request, CancellationToken ct = default);
}

public class UpdateUniteEnseignementService(
    ErpUnivDbContext db,
    IValidator<UpdateUniteEnseignementRequest> validator) : IUpdateUniteEnseignementService
{
    public async Task<UpdateUniteEnseignementResponse> ExecuteAsync(Guid id, UpdateUniteEnseignementRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        var ue = await db.UniteEnseignements
            .Include(u => u.FiliereNiveau)
                .ThenInclude(fn => fn.Filiere)
            .Include(u => u.FiliereNiveau)
                .ThenInclude(fn => fn.Niveau)
            .FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new NotFoundException($"L'unité d'enseignement avec l'identifiant '{id}' n'existe pas.");

        if (request.FiliereNiveauId.HasValue && request.FiliereNiveauId.Value != ue.FiliereNiveauId)
        {
            var newFiliereNiveau = await db.FiliereNiveaux
                .Include(fn => fn.Filiere)
                .Include(fn => fn.Niveau)
                .FirstOrDefaultAsync(fn => fn.Id == request.FiliereNiveauId.Value, ct)
                ?? throw new NotFoundException($"La filière-niveau avec l'identifiant '{request.FiliereNiveauId.Value}' n'existe pas.");

            ue.FiliereNiveauId = request.FiliereNiveauId.Value;
            ue.FiliereNiveau = newFiliereNiveau;
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var newCode = request.Code.Trim().ToUpperInvariant();
            if (newCode != ue.Code)
            {
                var codeExiste = await db.UniteEnseignements.AnyAsync(u => u.Code == newCode && u.Id != id, ct);
                if (codeExiste)
                {
                    throw new ConflictException($"Une unité d'enseignement avec le code '{newCode}' existe déjà.");
                }

                ue.Code = newCode;
            }
        }

        var targetNom = request.Nom.Trim();
        var nomNormalise = targetNom.ToLower();
        var nomExiste = await db.UniteEnseignements.AnyAsync(
            u => u.Id != id &&
                 u.FiliereNiveauId == ue.FiliereNiveauId &&
                 u.Semestre == request.Semestre &&
                 u.Nom.ToLowerInvariant() == nomNormalise,
            ct);

        if (nomExiste)
        {
            throw new ConflictException($"Une unité d'enseignement nommée '{targetNom}' existe déjà pour ce niveau et ce semestre.");
        }

        ue.Nom = targetNom;
        ue.Credit = request.Credit;
        ue.Semestre = request.Semestre;

        await db.SaveChangesAsync(ct);

        return ue.Adapt<UpdateUniteEnseignementResponse>();
    }
}

