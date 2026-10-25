namespace erp_univ_back.Features.Etablissement.Update;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IUpdateEtablissementService
{
    Task<UpdateEtablissementResponse> ExecuteAsync(Guid id, UpdateEtablissementRequest request, CancellationToken ct = default);
}

public class UpdateEtablissementService(ErpUnivDbContext db, IValidator<UpdateEtablissementRequest> validator) : IUpdateEtablissementService
{
    public async Task<UpdateEtablissementResponse> ExecuteAsync(Guid id, UpdateEtablissementRequest request, CancellationToken ct = default)
    {
        var verification = await validator.ValidateAsync(request, ct);
        if (!verification.IsValid)
            throw new BadRequestException(verification.Errors.First().ErrorMessage);

        var etablissement = await db.Etablissements.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Etablissement non trouvé");

        // Vérification d'unicité avec les autres établissements
        if (await db.Etablissements.AnyAsync(e => e.Id != id && e.Nom == request.Nom, ct))
            throw new ConflictException($"Un établissement avec le nom '{request.Nom}' existe déjà.");

        if (await db.Etablissements.AnyAsync(e => e.Id != id && e.Email == request.Email, ct))
            throw new ConflictException($"L'adresse email '{request.Email}' est déjà utilisée par un autre établissement.");

        if (await db.Etablissements.AnyAsync(e => e.Id != id && e.Telephone == request.Telephone, ct))
            throw new ConflictException($"Le numéro de téléphone '{request.Telephone}' est déjà enregistré.");

        request.Adapt(etablissement);
        await db.SaveChangesAsync(ct);

        return etablissement.Adapt<UpdateEtablissementResponse>();
    }
}
