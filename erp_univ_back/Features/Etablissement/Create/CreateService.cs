using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etablissement.Create
{
    public interface ICreateService
    {
        Task<EtablissementResponse> ExecuteAsync(EtablissementRequest request, CancellationToken ct = default);
    }

    public class CreateService(ErpUnivDbContext _db) : ICreateService
    {
        public async Task<EtablissementResponse> ExecuteAsync(EtablissementRequest request, CancellationToken ct = default)
        {
            if (await _db.Etablissements.AnyAsync(e => e.Nom == request.Nom, ct))
                throw new ConflictException($"Un établissement avec le nom '{request.Nom}' existe déjà.");

            if (await _db.Etablissements.AnyAsync(e => e.Email == request.Email, ct))
                throw new ConflictException($"L'adresse email '{request.Email}' est déjà utilisée par un autre établissement.");

            if (await _db.Etablissements.AnyAsync(e => e.Telephone == request.Telephone, ct))
                throw new ConflictException($"Le numéro de téléphone '{request.Telephone}' est déjà enregistré.");

            var entity = request.Adapt<EtablissementEntity>();
            _db.Etablissements.Add(entity);
            await _db.SaveChangesAsync(ct);

            return entity.Adapt<EtablissementResponse>();
        }
    }
}
