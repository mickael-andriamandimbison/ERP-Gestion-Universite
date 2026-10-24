namespace erp_univ_back.Features.Inscription.Create;

using erp_univ_back.Features.Inscription;

public record CreateInscriptionRequest(
    Guid EtudiantId,
    Guid FiliereNiveauId,
    Guid AnneeAcademiqueId
);

public record CreateInscriptionResponse(
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
