using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.Update;

public interface IUpdateFiliereService
{
    Task<UpdateFiliereResponse> ExecuteAsync(Guid id, UpdateFiliereRequest request, CancellationToken ct = default);
}

public class UpdateFiliereService(ErpUnivDbContext _db, IValidator<UpdateFiliereRequest> _validator) : IUpdateFiliereService
{
    public async Task<UpdateFiliereResponse> ExecuteAsync(Guid id, UpdateFiliereRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);

        var filiere = await _db.Filieres.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new NotFoundException($"Filière avec l'identifiant '{id}' introuvable.");

        var codeUpper = request.Code.Trim().ToUpperInvariant();
        var nomTrimmed = request.Nom.Trim();

        if (await _db.Filieres.AnyAsync(f => f.Id != id && f.Code == codeUpper, ct))
            throw new ConflictException($"Une autre filière avec le code '{codeUpper}' existe déjà.");

        filiere.Code = codeUpper;
        filiere.Nom = nomTrimmed;
        filiere.Description = request.Description?.Trim();

        await _db.SaveChangesAsync(ct);

        return filiere.Adapt<UpdateFiliereResponse>();
    }
}
