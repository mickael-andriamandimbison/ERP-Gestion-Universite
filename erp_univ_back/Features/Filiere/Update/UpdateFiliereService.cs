using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.Update;

public interface IUpdateFiliereService
{
    Task<UpdateFiliereResponse> ExecuteAsync(Guid id, UpdateFiliereRequest request, CancellationToken ct = default);
}

public class UpdateFiliereService(ErpUnivDbContext _db) : IUpdateFiliereService
{
    public async Task<UpdateFiliereResponse> ExecuteAsync(Guid id, UpdateFiliereRequest request, CancellationToken ct = default)
    {
        var filiere = await _db.Filieres.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new NotFoundException($"Filière avec l'identifiant '{id}' introuvable.");

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

        if (await _db.Filieres.AnyAsync(f => f.Id != id && f.Code == codeUpper, ct))
            throw new ConflictException($"Une autre filière avec le code '{codeUpper}' existe déjà.");

        filiere.Code = codeUpper;
        filiere.Nom = nomTrimmed;
        filiere.Description = request.Description?.Trim();

        await _db.SaveChangesAsync(ct);

        return filiere.Adapt<UpdateFiliereResponse>();
    }
}
