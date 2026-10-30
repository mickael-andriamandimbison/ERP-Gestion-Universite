namespace erp_univ_back.Features.AnneeAcademique.GetAll;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetAllAnneeAcademiqueService
{
    Task<List<GetAllAnneeAcademiqueResponse>> ExecuteAsync(GetAllAnneeAcademiqueRequest request, CancellationToken ct = default);
}

public class GetAllAnneeAcademiqueService(ErpUnivDbContext _db, IValidator<GetAllAnneeAcademiqueRequest> _validator) : IGetAllAnneeAcademiqueService
{
    public async Task<List<GetAllAnneeAcademiqueResponse>> ExecuteAsync(GetAllAnneeAcademiqueRequest request, CancellationToken ct = default)
    {
        var verification = await _validator.ValidateAsync(request, ct);
        if (!verification.IsValid)
            throw new BadRequestException(verification.Errors.First().ErrorMessage);

        var query = _db.AnneeAcademiques.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var isInMemory = _db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
            if (isInMemory)
            {
                var recherche = request.Recherche.Trim();
                query = query.Where(a => a.Libelle.Contains(recherche, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var motif = QueryHelper.VersMotifRecherche(request.Recherche);
                query = query.Where(a => EF.Functions.ILike(a.Libelle, motif));
            }
        }

        if (request.EstActive.HasValue)
        {
            query = query.Where(a => a.EstActive == request.EstActive.Value);
        }

        var entities = await query
            .OrderByDescending(a => a.EstActive)
            .ThenByDescending(a => a.DateDebut)
            .ToListAsync(ct);

        return entities.Adapt<List<GetAllAnneeAcademiqueResponse>>();
    }
}
