using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Professeur.Create
{
    public interface ICreateService
    {
        Task<ProfesseurEntity> ExecuteAsync(CreateProfRequest request);
    }
    public class CreateService(ErpUnivDbContext _db, IMatriculeGenerator _matricule) : ICreateService
    {
        public async Task<ProfesseurEntity> ExecuteAsync(CreateProfRequest request)
        {
            if (await _db.Professeurs.AnyAsync(x => x.Email == request.Email)) throw new ConflictException($"L'email {request.Email} est deja utilisé");

            if (await _db.Professeurs.AnyAsync(x => x.NumeroTelephone == request.NumeroTelephone)) throw new ConflictException($"Le numero {request.NumeroTelephone} est deja utilisé");

            if (await _db.Professeurs.AnyAsync(x => x.NumeroCin == request.NumeroCin)) throw new ConflictException($"Le cin {request.NumeroCin} doit être unique");

            var matricule = await _matricule.GenererAsync(false);

            var prof = request.Adapt<ProfesseurEntity>();
            prof.Matricule = matricule;

            _db.Professeurs.Add(prof);

            await _db.SaveChangesAsync();

            return prof;

        }
    }
}