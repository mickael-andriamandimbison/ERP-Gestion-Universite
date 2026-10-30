namespace erp_univ_back.Features.Filiere.GetAll;

public record GetAllFiliereRequest(
    string? Recherche = null,
    string ChampTri = "Nom",
    bool TriDecroissant = false,
    int Page = 1,
    int TaillePage = 10
);

public record GetAllFilieresRequest(
    string? Recherche = null,
    string ChampTri = "Nom",
    bool TriDecroissant = false,
    int Page = 1,
    int TaillePage = 10
) : GetAllFiliereRequest(Recherche, ChampTri, TriDecroissant, Page, TaillePage);

public record GetAllFiliereResponse(
    Guid Id,
    string Code,
    string Nom,
    string? Description,
    DateTime DateCreation,
    DateTime DateModification
);

public record FiliereGetAllResponse(
    Guid Id,
    string Code,
    string Nom,
    string? Description,
    DateTime DateCreation,
    DateTime DateModification
) : GetAllFiliereResponse(Id, Code, Nom, Description, DateCreation, DateModification);
