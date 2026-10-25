using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using erp_univ_back.Shared.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Professeur.GetAll;

public interface IGetAllProfesseurService
{
    Task<PagedResponse<GetAllProfesseurResponse>> ExecuteAsync(GetAllProfesseurRequest request, CancellationToken ct = default);
}

public class GetAllProfesseurService(ErpUnivDbContext _db) : IGetAllProfesseurService
{
    private const int TaillePageMax = 100;

    public async Task<PagedResponse<GetAllProfesseurResponse>> ExecuteAsync(GetAllProfesseurRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var taillePage = request.TaillePage < 1 ? 10 : Math.Min(request.TaillePage, TaillePageMax);

        var query = _db.Professeurs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var motif = QueryHelper.VersMotifRecherche(request.Recherche);
            query = query.Where(p =>
                EF.Functions.ILike(p.Nom, motif) ||
                (p.Prenom != null && EF.Functions.ILike(p.Prenom, motif)) ||
                EF.Functions.ILike(p.Matricule, motif) ||
                (p.Email != null && EF.Functions.ILike(p.Email, motif)) ||
                EF.Functions.ILike(p.NumeroTelephone, motif) ||
                EF.Functions.ILike(p.NumeroCin, motif));
        }

        query = (request.ChampTri?.ToLower(), request.TriDecroissant) switch
        {
            ("matricule", false) => query.OrderBy(p => p.Matricule).ThenByDescending(p => p.DateCreation),
            ("matricule", true) => query.OrderByDescending(p => p.Matricule).ThenByDescending(p => p.DateCreation),
            ("prenom", false) => query.OrderBy(p => p.Prenom).ThenByDescending(p => p.DateCreation),
            ("prenom", true) => query.OrderByDescending(p => p.Prenom).ThenByDescending(p => p.DateCreation),
            ("email", false) => query.OrderBy(p => p.Email).ThenByDescending(p => p.DateCreation),
            ("email", true) => query.OrderByDescending(p => p.Email).ThenByDescending(p => p.DateCreation),
            ("datecreation", false) => query.OrderBy(p => p.DateCreation),
            ("datecreation", true) => query.OrderByDescending(p => p.DateCreation),
            (_, true) => query.OrderByDescending(p => p.Nom).ThenByDescending(p => p.DateCreation),
            _ => query.OrderBy(p => p.Nom).ThenByDescending(p => p.DateCreation)
        };

        var totalElements = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalElements / (double)taillePage);

        var entities = await query
            .Skip((page - 1) * taillePage)
            .Take(taillePage)
            .ToListAsync(ct);

        return new PagedResponse<GetAllProfesseurResponse>(
            entities.Adapt<List<GetAllProfesseurResponse>>(),
            page,
            taillePage,
            totalElements,
            totalPages
        );
    }
}
