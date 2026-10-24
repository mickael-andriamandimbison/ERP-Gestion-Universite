namespace erp_univ_back.Features.Inscription.ChangeStatut;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IChangeStatutInscriptionService
{
    Task<ChangeStatutInscriptionResponse> ExecuteAsync(Guid id, ChangeStatutInscriptionRequest request, CancellationToken ct = default);
}

public class ChangeStatutInscriptionService(
    ErpUnivDbContext db,
    IValidator<ChangeStatutInscriptionRequest> validator) : IChangeStatutInscriptionService
{
    public async Task<ChangeStatutInscriptionResponse> ExecuteAsync(Guid id, ChangeStatutInscriptionRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        var inscription = await db.Inscriptions
            .Include(i => i.Etudiant)
            .Include(i => i.FiliereNiveau).ThenInclude(fn => fn.Filiere)
            .Include(i => i.FiliereNiveau).ThenInclude(fn => fn.Niveau)
            .Include(i => i.AnneeAcademique)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (inscription is null)
        {
            throw new NotFoundException($"L'inscription avec l'identifiant '{id}' n'existe pas.");
        }

        inscription.Statut = request.Statut;
        await db.SaveChangesAsync(ct);

        return inscription.Adapt<ChangeStatutInscriptionResponse>();
    }
}
