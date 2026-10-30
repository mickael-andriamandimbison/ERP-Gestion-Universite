using Microsoft.AspNetCore.Http;

namespace erp_univ_back.Features.Etudiant.Import;

public record ImportEtudiantsRequest
{
    public IFormFile? File { get; init; }
}

public record ImportFileRequest : ImportEtudiantsRequest;

public record ImportEtudiantLigneDto(
    string Nom = "",
    string? Prenom = null,
    string Email = "",
    string NumeroTelephone = "",
    string? NomPere = null,
    string? TelephonePere = null,
    string? ProfessionPere = null,
    string? EmailPere = null,
    string? NomMere = null,
    string? TelephoneMere = null,
    string? ProfessionMere = null,
    string? EmailMere = null,
    DateOnly? DateNaissance = null,
    string? LieuNaissance = null
)
{
    public ImportEtudiantLigneDto() : this("") { }
}

public record ImportLigneErreur(
    int Ligne,
    string Identifiant,
    string Message
);

public record ImportEtudiantReport(
    int TotalLignes,
    int Succes,
    int Echecs,
    List<ImportLigneErreur> Erreurs
);

public record ImportEtudiantsResponse(
    int TotalLignes,
    int Succes,
    int Echecs,
    List<ImportLigneErreur> Erreurs
);
