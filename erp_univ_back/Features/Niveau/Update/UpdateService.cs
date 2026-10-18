namespace erp_univ_back.Features.Niveau.Update;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IUpdateNiveauService
{
    Task<NiveauResponse> ExecuteAsync(Guid id, UpdateNiveauRequest request, CancellationToken ct = default);
}

public class UpdateNiveauService(ErpUnivDbContext db) : IUpdateNiveauService
{
    public async Task<NiveauResponse> ExecuteAsync(Guid id, UpdateNiveauRequest request, CancellationToken ct = default)
    {
        UpdateNiveauValidator.Valider(request);

        var niveau = await db.Niveaux.FirstOrDefaultAsync(n => n.Id == id, ct);
        if (niveau is null)
        {
            throw new NotFoundException($"Le niveau avec l'identifiant '{id}' n'existe pas.");
        }

        var trimmedCode = request.Code.Trim().ToUpperInvariant();
        if (trimmedCode != niveau.Code)
        {
            var codeExiste = await db.Niveaux.AnyAsync(n => n.Code == trimmedCode && n.Id != id, ct);
            if (codeExiste)
            {
                throw new ConflictException($"Un niveau avec le code '{trimmedCode}' existe déjà dans cet établissement.");
            }
        }

        niveau.Code = trimmedCode;
        niveau.Nom = request.Nom.Trim();
        niveau.Ordre = request.Ordre;

        await db.SaveChangesAsync(ct);

        return niveau.Adapt<NiveauResponse>();
    }
}
