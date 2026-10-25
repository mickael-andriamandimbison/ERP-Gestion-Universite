namespace erp_univ_back.Features.Etablissement.GetAll;

using erp_univ_back.Features.Etablissement;

public record GetAllEtablissementsRequest(
    string? Recherche = null,
    string? Ville = null,
    string ChampTri = "Nom",
    bool TriDecroissant = false,
    int Page = 1,
    int TaillePage = 10
);

public record GetAllEtablissementResponse(
    Guid Id,
    string Nom,
    string Adresse,
    string CodePostal,
    string Ville,
    string Pays,
    string Telephone,
    string Email,
    string? SiteWeb,
    string? Logo,
    string? Sigle,
    string? Description,
    StatusEtablissement Statut,
    DateTime DateCreation,
    DateTime DateModification
);
