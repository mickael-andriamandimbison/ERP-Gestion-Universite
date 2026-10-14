using System.ComponentModel.DataAnnotations;

namespace erp_univ_back.Features.Professeur.Update
{
    public record UpdateProfRequest(
        [Required, MaxLength(100)] string Nom,
        [MaxLength(100)] string? Prenom,
        [Required] DateOnly DateNaissance,
        [Required, MaxLength(100)] string LieuNaissance,
        [Required, MaxLength(20)] string NumeroTelephone,
        [EmailAddress] string? Email,
        [Required, MaxLength(17)] string NumeroCin
    );
}
