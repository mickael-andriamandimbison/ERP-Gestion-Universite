using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.AnneeAcademique.Create
{
    public interface ICreateService
    {
        Task<CreateAnneeAcademiqueResponse> ExecuteAsync(CreateAnneeAcademiqueRequest request, CancellationToken ct = default);
    }

    public class CreateService(ErpUnivDbContext _db) : ICreateService
    {
        public async Task<CreateAnneeAcademiqueResponse> ExecuteAsync(CreateAnneeAcademiqueRequest request, CancellationToken ct = default)
        {
            AnneeAcademiqueValidator.Valider(request.Libelle, request.DateDebut, request.DateFin);

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
}
