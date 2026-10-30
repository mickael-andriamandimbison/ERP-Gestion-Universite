using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Responses;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.GetAll;

public interface IGetAllFiliereService
{
    Task<PagedResponse<GetAllFiliereResponse>> ExecuteAsync(GetAllFiliereRequest request, CancellationToken ct = default);
}

public class GetAllFiliereService(ErpUnivDbContext _db, IValidator<GetAllFiliereRequest> _validator) : IGetAllFiliereService
{
    private const int TaillePageMax = 100;

    public async Task<PagedResponse<GetAllFiliereResponse>> ExecuteAsync(GetAllFiliereRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);

        var page = request.Page;
        var taillePage = Math.Min(request.TaillePage, TaillePageMax);

        var query = _db.Filieres.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var isInMemory = _db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
            if (isInMemory)
            {
                var r = request.Recherche.Trim();
                query = query.Where(f =>
                    f.Nom.Contains(r, StringComparison.OrdinalIgnoreCase) ||
                    f.Code.Contains(r, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var motif = QueryHelper.VersMotifRecherche(request.Recherche);
                query = query.Where(f =>
                    EF.Functions.ILike(f.Nom, motif) ||
                    EF.Functions.ILike(f.Code, motif));
            }
        }

        query = (request.ChampTri?.ToLower(), request.TriDecroissant) switch
        {
            ("code", false) => query.OrderBy(f => f.Code).ThenBy(f => f.Nom),
            ("code", true) => query.OrderByDescending(f => f.Code).ThenBy(f => f.Nom),
            ("datecreation", false) => query.OrderBy(f => f.DateCreation),
            ("datecreation", true) => query.OrderByDescending(f => f.DateCreation),
            (_, true) => query.OrderByDescending(f => f.Nom).ThenBy(f => f.Code),
            _ => query.OrderBy(f => f.Nom).ThenBy(f => f.Code)
        };

        var totalElements = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalElements / (double)taillePage);

        var entities = await query
            .Skip((page - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync(ct);

        return new PagedResponse<GetAllFiliereResponse>(
            entities.Adapt<List<GetAllFiliereResponse>>(),
            page,
            taillePage,
            totalElements,
            totalPages
        );
    }
}
