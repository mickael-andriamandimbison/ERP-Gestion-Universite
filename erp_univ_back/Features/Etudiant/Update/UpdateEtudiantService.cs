using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.Update;

public interface IUpdateEtudiantService
{
    Task<UpdateEtudiantResponse> ExecuteAsync(Guid id, UpdateEtudiantRequest request, CancellationToken ct = default);
}

public class UpdateEtudiantService(ErpUnivDbContext _db) : IUpdateEtudiantService
{
    public async Task<UpdateEtudiantResponse> ExecuteAsync(Guid id, UpdateEtudiantRequest request, CancellationToken ct = default)
    {
        var etudiant = await _db.Etudiants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Etudiant introuvable.");

        if (await _db.Etudiants.AnyAsync(x => x.Id != id && x.Email == request.Email, ct))
            throw new ConflictException($"L'adresse email '{request.Email}' est déjà utilisée par un autre étudiant.");

        if (await _db.Etudiants.AnyAsync(x => x.Id != id && x.NumeroTelephone == request.NumeroTelephone, ct))
            throw new ConflictException($"Le numéro de téléphone '{request.NumeroTelephone}' est déjà enregistré.");

        if (!string.IsNullOrWhiteSpace(request.NumeroCin) &&
            await _db.Etudiants.AnyAsync(x => x.Id != id && x.NumeroCin == request.NumeroCin, ct))
        {
            throw new ConflictException($"Le numéro CIN '{request.NumeroCin}' doit être unique.");
        }

        request.Adapt(etudiant);
        etudiant.NumeroTelephone = request.NumeroTelephone;
        etudiant.DateModification = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return etudiant.Adapt<UpdateEtudiantResponse>();
    }
}
