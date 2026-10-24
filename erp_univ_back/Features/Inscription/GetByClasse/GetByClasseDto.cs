namespace erp_univ_back.Features.Inscription.GetByClasse;

using erp_univ_back.Features.Inscription;

public record InscriptionByClasseResponse(
    Guid Id,
    Guid EtudiantId,
    string Matricule,
    string Nom,
    string? Prenom,
    string Email,
    string NumeroTelephone,
    StatutInscription Statut,
    DateTime DateInscription
);
