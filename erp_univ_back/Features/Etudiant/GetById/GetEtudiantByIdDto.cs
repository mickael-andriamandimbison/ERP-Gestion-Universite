namespace erp_univ_back.Features.Etudiant.GetById;

public record GetEtudiantByIdResponse(
    Guid Id,
    string Matricule,
    string Nom,
    string? Prenom,
    DateOnly DateNaissance,
    string LieuNaissance,
    string? NumeroCin,
    string NumeroTelephone,
    string Email,
    DateTime DateCreation,
    DateTime DateModification
);
