
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Niveau.Create
{

    public interface ICreateNiveauService
    {
        Task<CreateNiveauResponse> ExecuteAsync(CreateNiveauRequest request, CancellationToken ct = default);
    }

    public class CreateNiveauService(ErpUnivDbContext db) : ICreateNiveauService
    {
        public async Task<CreateNiveauResponse> ExecuteAsync(CreateNiveauRequest request, CancellationToken ct = default)
        {
            // 1. Validation de forme et des règles d'entrée
            CreateNiveauValidator.Valider(request);

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
}