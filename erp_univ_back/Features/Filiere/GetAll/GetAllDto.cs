namespace erp_univ_back.Features.Filiere.GetAll
{
    public record GetAllFilieresRequest(
        string? Recherche = null,
        string ChampTri = "Nom",
        bool TriDecroissant = false,
        int Page = 1,
        int TaillePage = 10
    );

    public record FiliereGetAllResponse(
        Guid Id,
        string Code,
        string Nom,
        string? Description,
        DateTime DateCreation,
        DateTime DateModification
    );
}
