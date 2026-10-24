namespace erp_univ_back.Features.Professeur.GetById;

public record GetProfesseurByIdResponse(
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

public record ProfesseurGetByIdResponse(
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
) : GetProfesseurByIdResponse(Id, Matricule, Nom, Prenom, NumeroTelephone, Email, NumeroCin, DateNaissance, LieuNaissance, DateCreation, DateModification);
