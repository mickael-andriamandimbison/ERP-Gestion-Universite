namespace erp_univ_back.Features.Matiere.GetPaginated;

public record GetMatieresPaginatedRequest(
    string? Search = null,
    int Page = 1,
    int PageSize = 10
);

