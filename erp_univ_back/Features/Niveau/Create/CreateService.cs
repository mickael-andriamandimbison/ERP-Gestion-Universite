namespace erp_univ_back.Features.Niveau.Create;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface ICreateNiveauService
{
    Task<CreateNiveauResponse> ExecuteAsync(CreateNiveauRequest request, CancellationToken ct = default);
}

public class CreateNiveauService(
    ErpUnivDbContext db,
    IValidator<CreateNiveauRequest> validator) : ICreateNiveauService
{
    public async Task<CreateNiveauResponse> ExecuteAsync(CreateNiveauRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        var trimmedCode = request.Code.Trim().ToUpperInvariant();

        var codeExiste = await db.Niveaux.AnyAsync(n => n.Code == trimmedCode, ct);
        if (codeExiste)
        {
            throw new ConflictException($"Un niveau avec le code '{trimmedCode}' existe déjà dans cet établissement.");
        }

        var niveau = new NiveauEntity
        {
            Code = trimmedCode,
            Nom = request.Nom.Trim(),
            Ordre = request.Ordre
        };

        db.Niveaux.Add(niveau);

        await db.SaveChangesAsync(ct);

        return niveau.Adapt<CreateNiveauResponse>();
    }
}