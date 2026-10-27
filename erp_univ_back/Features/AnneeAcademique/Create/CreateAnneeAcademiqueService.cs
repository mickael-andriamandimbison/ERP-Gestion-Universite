namespace erp_univ_back.Features.AnneeAcademique.Create;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface ICreateAnneeAcademiqueService
{
    Task<CreateAnneeAcademiqueResponse> ExecuteAsync(CreateAnneeAcademiqueRequest request, CancellationToken ct = default);
}

public class CreateAnneeAcademiqueService(ErpUnivDbContext _db, IValidator<CreateAnneeAcademiqueRequest> _validator) : ICreateAnneeAcademiqueService
{
    public async Task<CreateAnneeAcademiqueResponse> ExecuteAsync(CreateAnneeAcademiqueRequest request, CancellationToken ct = default)
    {
        var verification = await _validator.ValidateAsync(request, ct);
        if (!verification.IsValid)
            throw new BadRequestException(verification.Errors.First().ErrorMessage);

        var trimmedLibelle = request.Libelle.Trim();

        if (await _db.AnneeAcademiques.AnyAsync(a => a.Libelle == trimmedLibelle, ct))
            throw new ConflictException($"Une année académique avec le libellé '{trimmedLibelle}' existe déjà.");

        if (request.EstActive)
        {
            var anneesActives = await _db.AnneeAcademiques.Where(a => a.EstActive).ToListAsync(ct);
            foreach (var annee in anneesActives)
            {
                annee.EstActive = false;
            }
        }

        var entity = request.Adapt<AnneeAcademiqueEntity>();
        entity.Libelle = trimmedLibelle;

        _db.AnneeAcademiques.Add(entity);
        await _db.SaveChangesAsync(ct);

        return entity.Adapt<CreateAnneeAcademiqueResponse>();
    }
}
