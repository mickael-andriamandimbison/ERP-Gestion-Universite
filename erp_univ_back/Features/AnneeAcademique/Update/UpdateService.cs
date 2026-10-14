using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.AnneeAcademique.Update
{
    public interface IUpdateService
    {
        Task<UpdateAnneeAcademiqueResponse> ExecuteAsync(Guid id, UpdateAnneeAcademiqueRequest request, CancellationToken ct = default);
    }

    public class UpdateService(ErpUnivDbContext _db) : IUpdateService
    {
        public async Task<UpdateAnneeAcademiqueResponse> ExecuteAsync(Guid id, UpdateAnneeAcademiqueRequest request, CancellationToken ct = default)
        {
            var year = await _db.AnneeAcademiques.FirstOrDefaultAsync(a => a.Id == id, ct)
                ?? throw new NotFoundException("Année académique introuvable.");

            AnneeAcademiqueValidator.Valider(request.Libelle, request.DateDebut, request.DateFin);

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
}
