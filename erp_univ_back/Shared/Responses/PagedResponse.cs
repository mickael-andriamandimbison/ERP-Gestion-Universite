namespace erp_univ_back.Shared.Responses;

public record PagedResponse<T>(
    IEnumerable<T> Elements,
    int Page,
    int TaillePage,
    int TotalElements,
    int TotalPages
)
{
    public bool APagePrecedente => Page > 1;
    public bool APageSuivante => Page < TotalPages;
}
