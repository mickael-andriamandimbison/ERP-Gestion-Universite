using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.Create;

public interface ICreateFiliereService
{
    Task<CreateFiliereResponse> ExecuteAsync(CreateFiliereRequest request, CancellationToken ct = default);
}

public class CreateFiliereService(ErpUnivDbContext _db, IValidator<CreateFiliereRequest> _validator) : ICreateFiliereService
{
    public async Task<CreateFiliereResponse> ExecuteAsync(CreateFiliereRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);

        var codeUpper = request.Code.Trim().ToUpperInvariant();
        var nomTrimmed = request.Nom.Trim();

        if (await _db.Filieres.AnyAsync(f => f.Code == codeUpper, ct))
            throw new ConflictException($"Une filière avec le code '{codeUpper}' existe déjà.");

        var entity = request.Adapt<FiliereEntity>();
        entity.Code = codeUpper;
        entity.Nom = nomTrimmed;
        entity.Description = request.Description?.Trim();

        _db.Filieres.Add(entity);
        await _db.SaveChangesAsync(ct);

        return entity.Adapt<CreateFiliereResponse>();
    }
}
