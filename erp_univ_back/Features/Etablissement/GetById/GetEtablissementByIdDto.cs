namespace erp_univ_back.Features.Etablissement.GetById;

using erp_univ_back.Features.Etablissement;

public record GetEtablissementByIdResponse(
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
