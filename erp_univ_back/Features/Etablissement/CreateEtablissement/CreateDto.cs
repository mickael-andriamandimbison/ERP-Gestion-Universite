namespace erp_univ_back.Features.Etablissement.CreateEtablissement;

using System.ComponentModel.DataAnnotations;
using erp_univ_back.Shared.Entities;

public class EtablissementRequest
{
    [Required, MaxLength(100)] public required string Nom { get; set; }
    [Required, MaxLength(200)] public required string Adresse { get; set; }
    [Required, MaxLength(10)] public required string CodePostal { get; set; }
    [Required, MaxLength(50)] public required string Ville { get; set; }
    [Required, MaxLength(50)] public required string Pays { get; set; }
    [Required, MaxLength(20)] public required string Telephone { get; set; }
    [Required, EmailAddress, MaxLength(100)] public required string Email { get; set; }
    [Url, MaxLength(100)] public string? SiteWeb { get; set; }
    [MaxLength(200)] public string? Logo { get; set; }
    [MaxLength(30)] public string? Sigle { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public StatusEtablissement Statut { get; set; } = StatusEtablissement.Actif;
}

public class EtablissementResponse
{
    public Guid Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string CodePostal { get; set; } = string.Empty;
    public string Ville { get; set; } = string.Empty;
    public string Pays { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? SiteWeb { get; set; }
    public string? Logo { get; set; }
    public string? Sigle { get; set; }
    public string? Description { get; set; }
    public StatusEtablissement Statut { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime DateModification { get; set; }
}