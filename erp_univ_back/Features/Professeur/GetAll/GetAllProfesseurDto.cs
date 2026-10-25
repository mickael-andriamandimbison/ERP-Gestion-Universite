using erp_univ_back.Shared.Responses;

namespace erp_univ_back.Features.Professeur.GetAll;

public record GetAllProfesseurRequest(
    string? Recherche = null,
    string ChampTri = "Nom",
    bool TriDecroissant = false,
    int Page = 1,
    int TaillePage = 10
);

public record GetAllProfesseursRequest(
    string? Recherche = null,
    string ChampTri = "Nom",
    bool TriDecroissant = false,
    int Page = 1,
    int TaillePage = 10
) : GetAllProfesseurRequest(Recherche, ChampTri, TriDecroissant, Page, TaillePage);

public record GetAllProfesseurResponse(
    Guid Id,
    string Matricule,
    string Nom,
    string? Prenom,
    string NumeroTelephone,
    string? Email,
    string NumeroCin,
    DateOnly DateNaissance,
    string LieuNaissance,
    DateTime DateCreation,
    DateTime DateModification
);

public record ProfesseurResponse(
    Guid Id,
    string Matricule,
    string Nom,
    string? Prenom,
    string NumeroTelephone,
    string? Email,
    string NumeroCin,
    DateOnly DateNaissance,
    string LieuNaissance,
    DateTime DateCreation,
    DateTime DateModification
) : GetAllProfesseurResponse(Id, Matricule, Nom, Prenom, NumeroTelephone, Email, NumeroCin, DateNaissance, LieuNaissance, DateCreation, DateModification);
