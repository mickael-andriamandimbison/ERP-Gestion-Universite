using System.ComponentModel.DataAnnotations;
namespace erp_univ_back.Features.Etablissement.Create
{
    public record EtablissementRequest(
        [Required, MaxLength(100)] string Nom,
        [Required, MaxLength(200)] string Adresse,
        [Required, MaxLength(10)] string CodePostal,
        [Required, MaxLength(50)] string Ville,
        [Required, MaxLength(50)] string Pays,
        [Required, MaxLength(20)] string Telephone,
        [Required, EmailAddress, MaxLength(100)] string Email,
        [Url, MaxLength(100)] string? SiteWeb = null,
        [MaxLength(200)] string? Logo = null,
        [MaxLength(30)] string? Sigle = null,
        [MaxLength(500)] string? Description = null,
        StatusEtablissement Statut = StatusEtablissement.Actif
    );

    public record EtablissementResponse(
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
}


