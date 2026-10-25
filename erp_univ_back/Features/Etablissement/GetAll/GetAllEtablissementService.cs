namespace erp_univ_back.Features.Etablissement.GetAll;

using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Responses;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

public interface IGetAllEtablissementService
{
    Task<PagedResponse<GetAllEtablissementResponse>> ExecuteAsync(GetAllEtablissementsRequest request, CancellationToken ct = default);
}

public class GetAllEtablissementService(ErpUnivDbContext db, IValidator<GetAllEtablissementsRequest> validator) : IGetAllEtablissementService
{
    private const int TaillePageMax = 100;

    public async Task<PagedResponse<GetAllEtablissementResponse>> ExecuteAsync(
        GetAllEtablissementsRequest request, CancellationToken ct = default)
    {
        var verification = await validator.ValidateAsync(request, ct);
        if (!verification.IsValid)
            throw new BadRequestException(verification.Errors.First().ErrorMessage);

        var page = request.Page;
        var taillePage = Math.Min(request.TaillePage, TaillePageMax);

        var query = db.Etablissements.AsNoTracking();

        var isInMemory = db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var recherche = request.Recherche.Trim();
            if (isInMemory)
            {
                query = query.Where(e =>
                    e.Nom.Contains(recherche, StringComparison.OrdinalIgnoreCase) ||
                    (e.Sigle != null && e.Sigle.Contains(recherche, StringComparison.OrdinalIgnoreCase)));
            }
            else
            {
                var motif = QueryHelper.VersMotifRecherche(request.Recherche);
                query = query.Where(e =>
                    EF.Functions.ILike(e.Nom, motif) ||
                    (e.Sigle != null && EF.Functions.ILike(e.Sigle, motif)));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Ville))
        {
            var villeFiltre = request.Ville.Trim();
            if (isInMemory)
            {
                query = query.Where(e => e.Ville.Contains(villeFiltre, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var ville = QueryHelper.Echapper(villeFiltre);
                query = query.Where(e => EF.Functions.ILike(e.Ville, ville));
            }
        }

        query = (request.ChampTri?.ToLowerInvariant(), request.TriDecroissant) switch
        {
            ("ville", false) => query.OrderBy(e => e.Ville).ThenByDescending(e => e.DateCreation),
            ("ville", true) => query.OrderByDescending(e => e.Ville).ThenByDescending(e => e.DateCreation),
            ("datecreation", false) => query.OrderBy(e => e.DateCreation),
            ("datecreation", true) => query.OrderByDescending(e => e.DateCreation),
            (_, true) => query.OrderByDescending(e => e.Nom).ThenByDescending(e => e.DateCreation),
            _ => query.OrderBy(e => e.Nom).ThenByDescending(e => e.DateCreation)
        };

        var totalElements = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalElements / (double)taillePage);

        var entities = await query
            .Skip((page - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync(ct);

        return new PagedResponse<GetAllEtablissementResponse>(
            entities.Adapt<List<GetAllEtablissementResponse>>(),
            page,
            taillePage,
            totalElements,
            totalPages
        );
    }
}
