namespace erp_univ_back.Features.AnneeAcademique.Update;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IUpdateAnneeAcademiqueService
{
    Task<UpdateAnneeAcademiqueResponse> ExecuteAsync(Guid id, UpdateAnneeAcademiqueRequest request, CancellationToken ct = default);
}

public class UpdateAnneeAcademiqueService(ErpUnivDbContext _db, IValidator<UpdateAnneeAcademiqueRequest> _validator) : IUpdateAnneeAcademiqueService
{
    public async Task<UpdateAnneeAcademiqueResponse> ExecuteAsync(Guid id, UpdateAnneeAcademiqueRequest request, CancellationToken ct = default)
    {
        var verification = await _validator.ValidateAsync(request, ct);
        if (!verification.IsValid)
            throw new BadRequestException(verification.Errors.First().ErrorMessage);

        var year = await _db.AnneeAcademiques.FirstOrDefaultAsync(a => a.Id == id, ct)
            ?? throw new NotFoundException("Année académique introuvable.");

        var trimmedLibelle = request.Libelle.Trim();

        if (await _db.AnneeAcademiques.AnyAsync(a => a.Id != id && a.Libelle == trimmedLibelle, ct))
            throw new ConflictException($"Une année académique avec le libellé '{trimmedLibelle}' existe déjà.");

        year.Libelle = trimmedLibelle;
        year.DateDebut = request.DateDebut;
        year.DateFin = request.DateFin;
        year.DateModification = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return year.Adapt<UpdateAnneeAcademiqueResponse>();
    }
}
