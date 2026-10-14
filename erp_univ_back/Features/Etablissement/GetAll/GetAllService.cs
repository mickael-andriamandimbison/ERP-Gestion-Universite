using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using Mapster;
using Microsoft.EntityFrameworkCore;
using erp_univ_back.Features.Etablissement.Create;

namespace erp_univ_back.Features.Etablissement.GetAll
{
    public interface IGetAllService
    {
        Task<PagedResponse<EtablissementResponse>> ExecuteAsync(GetAllEtablissementsRequest request, CancellationToken ct);
    }
    public class GetAllService(ErpUnivDbContext db) : IGetAllService
    {
        private const int TaillePageMax = 100;

        public async Task<PagedResponse<EtablissementResponse>> ExecuteAsync(
            GetAllEtablissementsRequest request, CancellationToken ct = default)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var taillePage = request.TaillePage < 1 ? 10 : Math.Min(request.TaillePage, TaillePageMax);

            var query = db.Etablissements.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Recherche))
            {
                var motif = $"%{Echapper(request.Recherche.Trim())}%";
                query = query.Where(e =>
                    EF.Functions.ILike(e.Nom, motif) ||
                    (e.Sigle != null && EF.Functions.ILike(e.Sigle, motif)));
            }

            if (!string.IsNullOrWhiteSpace(request.Ville))
            {
                var ville = Echapper(request.Ville.Trim());
                query = query.Where(e => EF.Functions.ILike(e.Ville, ville));
            }

            query = (request.ChampTri?.ToLower(), request.TriDecroissant) switch
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

            return new PagedResponse<EtablissementResponse>(
                entities.Adapt<List<EtablissementResponse>>(),
                page,
                taillePage,
                totalElements,
                totalPages
            );
        }

        private static string Echapper(string valeur) =>
            valeur.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
    }
}

