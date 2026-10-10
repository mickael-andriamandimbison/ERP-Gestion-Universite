using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.Create
{
    public interface ICreateService
    {
        Task<CreateEtudiantResponse> ExecuteAsync(CreateEtudiantRequest request);
    }

    public class CreateService(ErpUnivDbContext _db, IMatriculeGenerator _matriculeGenerator) : ICreateService
    {
        public async Task<CreateEtudiantResponse> ExecuteAsync(CreateEtudiantRequest request)
        {
            if (await _db.Etudiants.AnyAsync(e => e.Email == request.Email))
                throw new ConflictException($"L'adresse email '{request.Email}' est déjà utilisée par un autre étudiant.");

            if (await _db.Etudiants.AnyAsync(e => e.NumeroTelphone == request.NumeroTelephone))
                throw new ConflictException($"Le numéro de téléphone '{request.NumeroTelephone}' est déjà enregistré.");

            var matricule = await _matriculeGenerator.GenererAsync();

            // Création de l'entité étudiant via Mapster
            var etudiant = request.Adapt<EtudiantEntity>();
            etudiant.Matricule = matricule;
            etudiant.NumeroTelphone = request.NumeroTelephone;
            etudiant.EtudiantParents = [];

            // Traitement des parents
            foreach (var parentDto in request.Parents)
            {
                // Chercher un parent existant avec ce numéro (même parent, plusieurs enfants)
                var parent = await _db.Parents.FirstOrDefaultAsync(p => p.NumeroTelphone == parentDto.NumeroTelephone);

                if (parent is null)
                {
                    // Parent inconnu → créer un nouveau
                    parent = parentDto.Adapt<ParentEntity>();
                    parent.NumeroTelphone = parentDto.NumeroTelephone;
                }

                // Créer le lien EtudiantParentEntity
                var lien = new EtudiantParentEntity
                {
                    Role = parentDto.Role,
                    Parent = parent,
                    Etudiant = etudiant
                };

                etudiant.EtudiantParents.Add(lien);
            }

            _db.Etudiants.Add(etudiant);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new ConflictException("Un conflit est survenu lors de l'enregistrement : email, téléphone ou données parent déjà existants.");
            }

            return new CreateEtudiantResponse(etudiant.Id, etudiant.Matricule);
        }
    }
}
