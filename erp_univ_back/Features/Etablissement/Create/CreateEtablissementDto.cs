namespace erp_univ_back.Features.Etablissement.Create;


public record CreateEtablissementRequest(
    string Nom,
    string Adresse,
    string CodePostal,
    string Ville,
    string Pays,
    string Telephone,
    string Email,
    string? SiteWeb = null,
    string? Logo = null,
    string? Sigle = null,
    string? Description = null,
    StatusEtablissement Statut = StatusEtablissement.Actif
);

public record CreateEtablissementResponse(
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
