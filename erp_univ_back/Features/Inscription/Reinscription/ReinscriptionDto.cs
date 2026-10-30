namespace erp_univ_back.Features.Inscription.Reinscription;

using erp_univ_back.Features.Inscription;

public record ReinscriptionRequest(
    Guid EtudiantId,
    Guid FiliereNiveauId,
    Guid AnneeAcademiqueId
);

public record ReinscriptionResponse(
    Guid Id,
    Guid EtudiantId,
    string EtudiantNom,
    string EtudiantPrenom,
    string EtudiantMatricule,
    Guid FiliereNiveauId,
    string ClasseLibelle,
    Guid AnneeAcademiqueId,
    string AnneeAcademiqueLibelle,
    StatutInscription Statut,
    DateTime DateCreation
);

