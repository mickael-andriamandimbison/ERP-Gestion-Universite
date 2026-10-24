namespace erp_univ_back.Features.FiliereNiveau.Create;

using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface ICreateFiliereNiveauService
{
    Task<CreateFiliereNiveauResponse> ExecuteAsync(CreateFiliereNiveauRequest request, CancellationToken ct = default);
}

public class CreateFiliereNiveauService(
    ErpUnivDbContext db,
    IValidator<CreateFiliereNiveauRequest> validator) : ICreateFiliereNiveauService
{
    public async Task<CreateFiliereNiveauResponse> ExecuteAsync(CreateFiliereNiveauRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        var filiere = await db.Filieres.FirstOrDefaultAsync(f => f.Id == request.FiliereId, ct)
            ?? throw new NotFoundException($"La filière avec l'identifiant '{request.FiliereId}' n'existe pas.");

        var niveau = await db.Niveaux.FirstOrDefaultAsync(n => n.Id == request.NiveauId, ct)
            ?? throw new NotFoundException($"Le niveau avec l'identifiant '{request.NiveauId}' n'existe pas.");

        var existe = await db.FiliereNiveaux.AnyAsync(
            fn => fn.FiliereId == request.FiliereId && fn.NiveauId == request.NiveauId,
            ct);

        if (existe)
        {
            throw new ConflictException($"L'association entre la filière '{filiere.Nom}' et le niveau '{niveau.Nom}' existe déjà.");
        }

        
        var entity = new FiliereNiveauEntity
        {
            FiliereId = request.FiliereId,
            NiveauId = request.NiveauId,
            LibelleAffiche = $"{filiere.Nom}-{niveau.Code}"
        };

        db.FiliereNiveaux.Add(entity);
        await db.SaveChangesAsync(ct);

        entity.Filiere = filiere;
        entity.Niveau = niveau;

        return entity.Adapt<CreateFiliereNiveauResponse>();
    }
}
