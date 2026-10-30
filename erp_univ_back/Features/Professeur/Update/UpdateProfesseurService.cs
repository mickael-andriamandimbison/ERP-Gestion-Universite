using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Professeur.Update;

public interface IUpdateProfesseurService
{
    Task<UpdateProfesseurResponse> ExecuteAsync(Guid id, UpdateProfesseurRequest request, CancellationToken ct = default);
}

public class UpdateProfesseurService(
    ErpUnivDbContext _db,
    IValidator<UpdateProfesseurRequest> _validator) : IUpdateProfesseurService
{
    public async Task<UpdateProfesseurResponse> ExecuteAsync(Guid id, UpdateProfesseurRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);

        var prof = await _db.Professeurs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Professeur introuvable.");

        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _db.Professeurs.AnyAsync(x => x.Id != id && x.Email == request.Email, ct))
        {
            throw new ConflictException($"L'email {request.Email} est déjà utilisé.");
        }

        if (await _db.Professeurs.AnyAsync(x => x.Id != id && x.NumeroTelephone == request.NumeroTelephone, ct))
        {
            throw new ConflictException($"Le numéro {request.NumeroTelephone} est déjà utilisé.");
        }

        if (await _db.Professeurs.AnyAsync(x => x.Id != id && x.NumeroCin == request.NumeroCin, ct))
        {
            throw new ConflictException($"Le CIN {request.NumeroCin} doit être unique.");
        }

        request.Adapt(prof);
        prof.DateModification = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return prof.Adapt<UpdateProfesseurResponse>();
    }
}
