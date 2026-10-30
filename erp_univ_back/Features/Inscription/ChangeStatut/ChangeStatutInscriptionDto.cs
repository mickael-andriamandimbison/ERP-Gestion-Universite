namespace erp_univ_back.Features.Inscription.ChangeStatut;

using erp_univ_back.Features.Inscription;

public record ChangeStatutInscriptionRequest(
    StatutInscription Statut
);

public record ChangeStatutRequest(
    StatutInscription Statut
) : ChangeStatutInscriptionRequest(Statut);

public record ChangeStatutInscriptionResponse(
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
    DateTime DateModification
);

public record ChangeStatutResponse(
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
    DateTime DateModification
) : ChangeStatutInscriptionResponse(Id, EtudiantId, EtudiantNom, EtudiantPrenom, EtudiantMatricule, FiliereNiveauId, ClasseLibelle, AnneeAcademiqueId, AnneeAcademiqueLibelle, Statut, DateModification);
