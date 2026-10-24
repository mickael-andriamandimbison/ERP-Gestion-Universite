using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.Create;

public interface ICreateEtudiantService
{
    Task<CreateEtudiantResponse> ExecuteAsync(CreateEtudiantRequest request, CancellationToken ct = default);
}

public class CreateEtudiantService(ErpUnivDbContext _db, IMatriculeGenerator _matriculeGenerator) : ICreateEtudiantService
{
    public async Task<CreateEtudiantResponse> ExecuteAsync(CreateEtudiantRequest request, CancellationToken ct = default)
    {
        if (await _db.Etudiants.AnyAsync(e => e.Email == request.Email, ct))
            throw new ConflictException($"L'adresse email '{request.Email}' est déjà utilisée par un autre étudiant.");

        if (await _db.Etudiants.AnyAsync(e => e.NumeroTelphone == request.NumeroTelephone, ct))
            throw new ConflictException($"Le numéro de téléphone '{request.NumeroTelephone}' est déjà enregistré.");

        var matricule = await _matriculeGenerator.GenererAsync(true, ct);

        // Création de l'entité étudiant via Mapster
        var etudiant = request.Adapt<EtudiantEntity>();
        etudiant.Matricule = matricule;
        etudiant.NumeroTelphone = request.NumeroTelephone;
        etudiant.EtudiantParents = [];

        // Traitement des parents
        foreach (var parentDto in request.Parents)
        {
            // Chercher un parent existant 
            var parent = await _db.Parents.FirstOrDefaultAsync(p => p.NumeroTelphone == parentDto.NumeroTelephone, ct);

            if (parent is null)
            {
                //créer un nouveau
                parent = parentDto.Adapt<ParentEntity>();
                parent.NumeroTelphone = parentDto.NumeroTelephone;
            }

            // Créer le lien EtudiantParent
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
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            var vraiMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            throw new ConflictException($"Conflit technique : {vraiMessage}");
        }

        return new CreateEtudiantResponse(etudiant.Id, etudiant.Matricule);
    }
}
