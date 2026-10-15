using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using erp_univ_back.Shared.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Filiere.GetAll
{
    public interface IGetAllService
    {
        Task<PagedResponse<FiliereGetAllResponse>> ExecuteAsync(GetAllFilieresRequest request, CancellationToken ct = default);
    }

    public class GetAllService(ErpUnivDbContext _db) : IGetAllService
    {
        private const int TaillePageMax = 100;

        public async Task<PagedResponse<FiliereGetAllResponse>> ExecuteAsync(GetAllFilieresRequest request, CancellationToken ct = default)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var taillePage = request.TaillePage < 1 ? 10 : Math.Min(request.TaillePage, TaillePageMax);

            var query = _db.Filieres.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Recherche))
            {
                var motif = QueryHelper.VersMotifRecherche(request.Recherche);
                query = query.Where(f =>
                    EF.Functions.ILike(f.Nom, motif) ||
                    EF.Functions.ILike(f.Code, motif));
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

            return new PagedResponse<FiliereGetAllResponse>(
                entities.Adapt<List<FiliereGetAllResponse>>(),
                page,
                taillePage,
                totalElements,
                totalPages
            );
        }
    }
}
