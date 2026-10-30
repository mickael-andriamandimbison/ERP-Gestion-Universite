using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.Create;

public interface ICreateEtudiantService
{
    Task<CreateEtudiantResponse> ExecuteAsync(CreateEtudiantRequest request, CancellationToken ct = default);
}

public class CreateEtudiantService(ErpUnivDbContext _db, IMatriculeGenerator _matriculeGenerator, IValidator<CreateEtudiantRequest> _validator) : ICreateEtudiantService
{
    public async Task<CreateEtudiantResponse> ExecuteAsync(CreateEtudiantRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);
        }

        if (await _db.Etudiants.AnyAsync(e => e.NumeroTelephone == request.NumeroTelephone, ct))
            throw new ConflictException($"Le numéro de téléphone '{request.NumeroTelephone}' est déjà enregistré.");

        if (await _db.Etudiants.AnyAsync(e => e.Email == request.Email, ct))
            throw new ConflictException($"L'adresse email '{request.Email}' est déjà utilisée.");

        if (!string.IsNullOrWhiteSpace(request.NumeroCin) &&
            await _db.Etudiants.AnyAsync(e => e.NumeroCin == request.NumeroCin, ct))
        {
            throw new ConflictException($"Le numéro CIN '{request.NumeroCin}' est déjà utilisé.");
        }

        var matricule = await _matriculeGenerator.GenererAsync(true, ct);

        var etudiant = request.Adapt<EtudiantEntity>();
        etudiant.Matricule = matricule;
        etudiant.EtudiantParents = [];

        foreach (var parentDto in request.Parents)
        {
            var parent = await _db.Parents.FirstOrDefaultAsync(p => p.NumeroTelphone == parentDto.NumeroTelephone, ct);

            if (parent is null)
            {
                parent = parentDto.Adapt<ParentEntity>();
                parent.NumeroTelphone = parentDto.NumeroTelephone;
            }

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
