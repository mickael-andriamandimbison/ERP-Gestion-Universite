using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Responses;
using erp_univ_back.Shared.Utils;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.GetAll;

public interface IGetAllEtudiantService
{
    Task<PagedResponse<GetAllEtudiantResponse>> ExecuteAsync(GetAllEtudiantRequest request, CancellationToken ct = default);
}

public class GetAllEtudiantService(ErpUnivDbContext _db) : IGetAllEtudiantService
{
    private const int TaillePageMax = 100;

    public async Task<PagedResponse<GetAllEtudiantResponse>> ExecuteAsync(GetAllEtudiantRequest request, CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var taillePage = request.TaillePage < 1 ? 10 : Math.Min(request.TaillePage, TaillePageMax);

        var query = _db.Etudiants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var motif = QueryHelper.VersMotifRecherche(request.Recherche);
            query = query.Where(e =>
                EF.Functions.ILike(e.Nom, motif) ||
                (e.Prenom != null && EF.Functions.ILike(e.Prenom, motif)) ||
                EF.Functions.ILike(e.Matricule, motif) ||
                EF.Functions.ILike(e.Email, motif) ||
                EF.Functions.ILike(e.NumeroTelephone, motif) ||
                (e.NumeroCin != null && EF.Functions.ILike(e.NumeroCin, motif)));
        }

        query = (request.ChampTri?.ToLower(), request.TriDecroissant) switch
        {
            ("matricule", false) => query.OrderBy(e => e.Matricule).ThenByDescending(e => e.DateCreation),
            ("matricule", true) => query.OrderByDescending(e => e.Matricule).ThenByDescending(e => e.DateCreation),
            ("prenom", false) => query.OrderBy(e => e.Prenom).ThenByDescending(e => e.DateCreation),
            ("prenom", true) => query.OrderByDescending(e => e.Prenom).ThenByDescending(e => e.DateCreation),
            ("email", false) => query.OrderBy(e => e.Email).ThenByDescending(e => e.DateCreation),
            ("email", true) => query.OrderByDescending(e => e.Email).ThenByDescending(e => e.DateCreation),
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

        return new PagedResponse<GetAllEtudiantResponse>(
            entities.Adapt<List<GetAllEtudiantResponse>>(),
            page,
            taillePage,
            totalElements,
            totalPages
        );
    }
}
