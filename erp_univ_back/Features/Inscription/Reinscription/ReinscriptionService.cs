namespace erp_univ_back.Features.Inscription.Reinscription;

using erp_univ_back.Features.Inscription;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IReinscriptionService
{
    Task<ReinscriptionResponse> ExecuteAsync(ReinscriptionRequest request, CancellationToken ct = default);
}

public interface IReinscriptionInscriptionService : IReinscriptionService
{
}

public class ReinscriptionService(
    ErpUnivDbContext db,
    IValidator<ReinscriptionRequest> validator) : IReinscriptionService, IReinscriptionInscriptionService
{
    public async Task<ReinscriptionResponse> ExecuteAsync(ReinscriptionRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            var firstError = validationResult.Errors.First().ErrorMessage;
            throw new BadRequestException(firstError);
        }

        var etudiant = await db.Etudiants.FirstOrDefaultAsync(e => e.Id == request.EtudiantId, ct);
        if (etudiant is null)
        {
            throw new NotFoundException($"L'étudiant avec l'identifiant '{request.EtudiantId}' n'existe pas.");
        }

        var classe = await db.FiliereNiveaux
            .Include(fn => fn.Niveau)
            .Include(fn => fn.Filiere)
            .FirstOrDefaultAsync(fn => fn.Id == request.FiliereNiveauId, ct);
        if (classe is null)
        {
            throw new NotFoundException($"La classe (filière-niveau) avec l'identifiant '{request.FiliereNiveauId}' n'existe pas.");
        }

        var annee = await db.AnneeAcademiques.FirstOrDefaultAsync(a => a.Id == request.AnneeAcademiqueId, ct);
        if (annee is null)
        {
            throw new NotFoundException($"L'année académique avec l'identifiant '{request.AnneeAcademiqueId}' n'existe pas.");
        }

        if (!annee.EstActive)
        {
            throw new BadRequestException($"L'année académique '{annee.Libelle}' n'est pas active. Les réinscriptions ne sont possibles que sur une année académique active.");
        }

        var dejaInscrit = await db.Inscriptions.AnyAsync(
            i => i.EtudiantId == request.EtudiantId
              && i.FiliereNiveauId == request.FiliereNiveauId
              && i.AnneeAcademiqueId == request.AnneeAcademiqueId,
            ct);

        if (dejaInscrit)
        {
            throw new ConflictException($"L'étudiant '{etudiant.Nom} {etudiant.Prenom}' est déjà inscrit dans cette classe pour l'année académique '{annee.Libelle}'.");
        }

        var inscription = request.Adapt<InscriptionEntity>();
        inscription.Statut = StatutInscription.Inscrit;

        db.Inscriptions.Add(inscription);
        await db.SaveChangesAsync(ct);

        inscription.Etudiant = etudiant;
        inscription.FiliereNiveau = classe;
        inscription.AnneeAcademique = annee;

        return inscription.Adapt<ReinscriptionResponse>();
    }
}
