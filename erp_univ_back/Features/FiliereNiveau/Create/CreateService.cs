namespace erp_univ_back.Features.FiliereNiveau.Create;

using erp_univ_back.Features.FiliereNiveau;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

public interface ICreateFiliereNiveauService
{
    Task<CreateFiliereNiveauResponse> ExecuteAsync(CreateFiliereNiveauRequest request, CancellationToken ct = default);
}

public class CreateFiliereNiveauService(ErpUnivDbContext db) : ICreateFiliereNiveauService
{
    public async Task<CreateFiliereNiveauResponse> ExecuteAsync(CreateFiliereNiveauRequest request, CancellationToken ct = default)
    {
        CreateFiliereNiveauValidator.Valider(request);

        var filiere = await db.Filieres.FirstOrDefaultAsync(f => f.Id == request.FiliereId, ct);
        if (filiere is null)
        {
            throw new NotFoundException($"La filière avec l'identifiant '{request.FiliereId}' n'existe pas.");
        }

        var niveau = await db.Niveaux.FirstOrDefaultAsync(n => n.Id == request.NiveauId, ct);
        if (niveau is null)
        {
            throw new NotFoundException($"Le niveau avec l'identifiant '{request.NiveauId}' n'existe pas.");
        }

        var existe = await db.FiliereNiveaux.AnyAsync(
            fn => fn.FiliereId == request.FiliereId && fn.NiveauId == request.NiveauId,
            ct);

        if (existe)
        {
            throw new ConflictException($"L'association entre la filière '{filiere.Nom}' et le niveau '{niveau.Nom}' existe déjà.");
        }

        var libelleFinal = string.IsNullOrWhiteSpace(request.LibelleAffiche)
            ? $"{niveau.Nom} {filiere.Nom}"
            : request.LibelleAffiche.Trim();

        var entity = new FiliereNiveauEntity
        {
            FiliereId = request.FiliereId,
            NiveauId = request.NiveauId,
            LibelleAffiche = libelleFinal
        };

        db.FiliereNiveaux.Add(entity);
        await db.SaveChangesAsync(ct);

        return new CreateFiliereNiveauResponse(
            entity.Id,
            entity.FiliereId,
            filiere.Nom,
            entity.NiveauId,
            niveau.Nom,
            entity.LibelleAffiche!,
            entity.DateCreation
        );
    }
}
