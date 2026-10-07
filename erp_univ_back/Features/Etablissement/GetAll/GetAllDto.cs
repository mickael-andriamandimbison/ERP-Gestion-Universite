namespace erp_univ_back.Features.Etablissement.GetAll;

public class GetAllEtablissementsRequest
{
    public string? Recherche { get; set; }
    public string? Ville { get; set; }

    public string ChampTri { get; set; } = "Nom";
    public bool TriDecroissant { get; set; } = false;

    public int Page { get; set; } = 1;
    public int TaillePage { get; set; } = 10;
}

public class PagedResponse<T>
{
    public IEnumerable<T> Elements { get; set; } = [];

    public int Page { get; set; }
    public int TaillePage { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }

    public bool APagePrecedente => Page > 1;
    public bool APageSuivante => Page < TotalPages;
}