namespace erp_univ_back.Features.Professeur.GetById;

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
);
