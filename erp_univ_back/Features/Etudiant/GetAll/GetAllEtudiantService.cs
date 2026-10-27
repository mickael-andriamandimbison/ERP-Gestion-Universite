using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Exceptions;
using erp_univ_back.Shared.Responses;
using erp_univ_back.Shared.Utils;
using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace erp_univ_back.Features.Etudiant.GetAll;

public interface IGetAllEtudiantService
{
    Task<PagedResponse<GetAllEtudiantResponse>> ExecuteAsync(GetAllEtudiantRequest request, CancellationToken ct = default);
}

public class GetAllEtudiantService(ErpUnivDbContext _db, IValidator<GetAllEtudiantRequest> _validator) : IGetAllEtudiantService
{
    private const int TaillePageMax = 100;

    public async Task<PagedResponse<GetAllEtudiantResponse>> ExecuteAsync(GetAllEtudiantRequest request, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new BadRequestException(validationResult.Errors.First().ErrorMessage);

        var page = request.Page;
        var taillePage = Math.Min(request.TaillePage, TaillePageMax);

        var query = _db.Etudiants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var isInMemory = _db.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
            if (isInMemory)
            {
                var r = request.Recherche.Trim();
                query = query.Where(e =>
                    e.Nom.Contains(r, StringComparison.OrdinalIgnoreCase) ||
                    (e.Prenom != null && e.Prenom.Contains(r, StringComparison.OrdinalIgnoreCase)) ||
                    e.Matricule.Contains(r, StringComparison.OrdinalIgnoreCase) ||
                    e.Email.Contains(r, StringComparison.OrdinalIgnoreCase) ||
                    e.NumeroTelephone.Contains(r, StringComparison.OrdinalIgnoreCase) ||
                    (e.NumeroCin != null && e.NumeroCin.Contains(r, StringComparison.OrdinalIgnoreCase)));
            }
            else
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
        }

        query = (request.ChampTri?.ToLowerInvariant(), request.TriDecroissant) switch
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
