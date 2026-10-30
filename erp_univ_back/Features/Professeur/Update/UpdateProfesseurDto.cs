using System.ComponentModel.DataAnnotations;

namespace erp_univ_back.Features.Professeur.Update;

public record UpdateProfesseurRequest(
    [Required, MaxLength(100)] string Nom,
    [MaxLength(100)] string? Prenom,
    [Required] DateOnly DateNaissance,
    [Required, MaxLength(100)] string LieuNaissance,
    [Required, MaxLength(20)] string NumeroTelephone,
    [EmailAddress] string? Email,
    [Required, MaxLength(17)] string NumeroCin
);

public record UpdateProfRequest(
    [Required, MaxLength(100)] string Nom,
    [MaxLength(100)] string? Prenom,
    [Required] DateOnly DateNaissance,
    [Required, MaxLength(100)] string LieuNaissance,
    [Required, MaxLength(20)] string NumeroTelephone,
    [EmailAddress] string? Email,
    [Required, MaxLength(17)] string NumeroCin
) : UpdateProfesseurRequest(Nom, Prenom, DateNaissance, LieuNaissance, NumeroTelephone, Email, NumeroCin);

public record UpdateProfesseurResponse(
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
