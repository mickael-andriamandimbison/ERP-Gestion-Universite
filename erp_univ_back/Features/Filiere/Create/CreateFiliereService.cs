using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.Create;

public interface ICreateFiliereService
{
    Task<CreateFiliereResponse> ExecuteAsync(CreateFiliereRequest request, CancellationToken ct = default);
}

public class CreateFiliereService(ErpUnivDbContext _db) : ICreateFiliereService
{
    public async Task<CreateFiliereResponse> ExecuteAsync(CreateFiliereRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            throw new BadRequestException("Le code de la filière est obligatoire.");

        if (string.IsNullOrWhiteSpace(request.Nom))
            throw new BadRequestException("Le nom de la filière est obligatoire.");

        var codeUpper = request.Code.Trim().ToUpperInvariant();
        if (codeUpper.Length > 20)
            throw new BadRequestException("Le code de la filière ne doit pas dépasser 20 caractères.");

        var nomTrimmed = request.Nom.Trim();
        if (nomTrimmed.Length > 150)
            throw new BadRequestException("Le nom de la filière ne doit pas dépasser 150 caractères.");

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
