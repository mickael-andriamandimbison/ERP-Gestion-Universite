namespace erp_univ_back.Shared.Models;

public record PaginatedList<T>(
    List<T> Items,
    int TotalCount,
    int CurrentPage,
    int TotalPages
);

