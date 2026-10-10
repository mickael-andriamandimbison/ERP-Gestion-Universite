namespace erp_univ_back.Features.Etablissement.GetAll
{
    public record GetAllEtablissementsRequest(
        string? Recherche = null,
        string? Ville = null,
        string ChampTri = "Nom",
        bool TriDecroissant = false,
        int Page = 1,
        int TaillePage = 10
    );

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
}

