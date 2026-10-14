using erp_univ_back.Shared.Responses;

namespace erp_univ_back.Features.Etudiant.GetAll
{
    public record GetAllEtudiantRequest(
        string? Recherche = null,
        string ChampTri = "Nom",
        bool TriDecroissant = false,
        int Page = 1,
        int TaillePage = 10
    );

    public record EtudiantGetAllResponse(
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