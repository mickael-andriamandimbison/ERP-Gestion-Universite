namespace erp_univ_back.Features.UniteEnseignement.Create;

using erp_univ_back.Features.UniteEnseignement;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface ICreateUniteEnseignementService
{
    Task<CreateUniteEnseignementResponse> ExecuteAsync(CreateUniteEnseignementRequest request, CancellationToken ct = default);
}

public class CreateUniteEnseignementService(
    ErpUnivDbContext db,
    IValidator<CreateUniteEnseignementRequest> validator,
    IUniteEnseignementCodeGenerator codeGenerator) : ICreateUniteEnseignementService
{
    public async Task<CreateUniteEnseignementResponse> ExecuteAsync(CreateUniteEnseignementRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        var filiereNiveau = await db.FiliereNiveaux
            .Include(fn => fn.Filiere)
            .Include(fn => fn.Niveau)
            .FirstOrDefaultAsync(fn => fn.Id == request.FiliereNiveauId, ct)
            ?? throw new NotFoundException($"La filière-niveau avec l'identifiant '{request.FiliereNiveauId}' n'existe pas.");

        string code;
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            code = request.Code.Trim().ToUpper();
            var codeExiste = await db.UniteEnseignements.AnyAsync(u => u.Code == code, ct);
            if (codeExiste)
            {
                throw new ConflictException($"Une unité d'enseignement avec le code '{code}' existe déjà.");
            }
        }
        else
        {
            code = await codeGenerator.GenererAsync(filiereNiveau.Id, ct);
        }

        var nomNormalise = request.Nom.Trim().ToLower();
        var nomExiste = await db.UniteEnseignements.AnyAsync(
            u => u.FiliereNiveauId == request.FiliereNiveauId &&
                 u.Semestre == request.Semestre &&
                 u.Nom.Equals(nomNormalise, StringComparison.InvariantCultureIgnoreCase),
            ct);

        if (nomExiste)
        {
            throw new ConflictException($"Une unité d'enseignement nommée '{request.Nom.Trim()}' existe déjà pour ce niveau et ce semestre.");
        }

        var entity = new UniteEnseignementEntity
        {
            Code = code,
            Nom = request.Nom.Trim(),
            Credit = request.Credit,
            Semestre = request.Semestre,
            FiliereNiveauId = request.FiliereNiveauId,
            FiliereNiveau = filiereNiveau
        };

        db.UniteEnseignements.Add(entity);
        await db.SaveChangesAsync(ct);

        return entity.Adapt<CreateUniteEnseignementResponse>();
    }
}

