using erp_univ_back.Shared.Responses;

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
}
