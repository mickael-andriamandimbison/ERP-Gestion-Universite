using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Professeur.Create;

public interface ICreateProfesseurService
{
    Task<CreateProfesseurResponse> ExecuteAsync(CreateProfesseurRequest request, CancellationToken ct = default);
}

public interface ICreateService : ICreateProfesseurService
{
}

public class CreateProfesseurService(
    ErpUnivDbContext _db,
    IMatriculeGenerator _matricule,
    IValidator<CreateProfesseurRequest> _validator) : ICreateProfesseurService, ICreateService
{
    public async Task<CreateProfesseurResponse> ExecuteAsync(CreateProfesseurRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _db.Professeurs.AnyAsync(x => x.Email == request.Email, ct))
        {
            throw new ConflictException($"L'email {request.Email} est deja utilisé");
        }

        if (await _db.Professeurs.AnyAsync(x => x.NumeroTelephone == request.NumeroTelephone, ct))
        {
            throw new ConflictException($"Le numero {request.NumeroTelephone} est deja utilisé");
        }

        if (await _db.Professeurs.AnyAsync(x => x.NumeroCin == request.NumeroCin, ct))
        {
            throw new ConflictException($"Le cin {request.NumeroCin} doit être unique");
        }

        var matricule = await _matricule.GenererAsync(false, ct);

        var prof = request.Adapt<ProfesseurEntity>();
        prof.Matricule = matricule;

        _db.Professeurs.Add(prof);

        await _db.SaveChangesAsync(ct);

        return prof.Adapt<CreateProfesseurResponse>();
    }
}
