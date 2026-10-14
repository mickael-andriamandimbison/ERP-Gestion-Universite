using System.ComponentModel.DataAnnotations;

namespace erp_univ_back.Features.Professeur.Create
{
    public record CreateProfRequest(
       string Nom,
       string? Prenom,
        DateOnly DateNaissance,
        string LieuNaissance,
       string NumeroTelephone,
       string? Email,
       string NumeroCin
   );

    public record CreateProfResponse();
}