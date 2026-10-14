using System.ComponentModel.DataAnnotations;

namespace erp_univ_back.Features.Etudiant.Update
{
    public record UpdateEtudiantRequest(
        [Required, MaxLength(100)] string Nom,
        [MaxLength(100)] string? Prenom,
        [Required] DateOnly DateNaissance,
        [Required, MaxLength(100)] string LieuNaissance,
        [MaxLength(17)] string? NumeroCin,
        [Required, MaxLength(20)] string NumeroTelephone,
        [Required, EmailAddress] string Email
    );

    public record UpdateEtudiantResponse(
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
}
