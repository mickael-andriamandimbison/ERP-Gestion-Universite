namespace erp_univ_back.Features.Matiere.GetPaginated;

using erp_univ_back.Features.Matiere.GetByUeId;
using erp_univ_back.Shared.Data;
using erp_univ_back.Shared.Models;
using Microsoft.EntityFrameworkCore;

public interface IGetMatieresPaginatedService
{
    Task<PaginatedList<MatiereItemDto>> ExecuteAsync(GetMatieresPaginatedRequest request, CancellationToken ct = default);
}

public class GetMatieresPaginatedService(ErpUnivDbContext db) : IGetMatieresPaginatedService
{
    public async Task<PaginatedList<MatiereItemDto>> ExecuteAsync(GetMatieresPaginatedRequest request, CancellationToken ct = default)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var query = db.Matieres.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(m => m.Nom.ToLower().Contains(search) || m.Code.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var items = await query
            .OrderBy(m => m.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MatiereItemDto(
                m.Id,
                m.Code,
                m.Nom,
                m.Coefficient
            ))
            .ToListAsync(ct);

        return new PaginatedList<MatiereItemDto>(
            Items: items,
            TotalCount: totalCount,
            CurrentPage: page,
            TotalPages: totalPages
        );
    }
}

