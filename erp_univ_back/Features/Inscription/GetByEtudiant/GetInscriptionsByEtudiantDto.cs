namespace erp_univ_back.Features.Inscription.GetByEtudiant;

using erp_univ_back.Features.Inscription;

public record GetInscriptionsByEtudiantResponse(
    Guid Id,
    Guid FiliereNiveauId,
    string FiliereNom,
    string NiveauNom,
    string ClasseLibelle,
    Guid AnneeAcademiqueId,
    string AnneeAcademiqueLibelle,
    StatutInscription Statut,
    DateTime DateInscription
);

public record InscriptionByEtudiantResponse(
    Guid Id,
    Guid FiliereNiveauId,
    string FiliereNom,
    string NiveauNom,
    string ClasseLibelle,
    Guid AnneeAcademiqueId,
    string AnneeAcademiqueLibelle,
    StatutInscription Statut,
    DateTime DateInscription
) : GetInscriptionsByEtudiantResponse(Id, FiliereNiveauId, FiliereNom, NiveauNom, ClasseLibelle, AnneeAcademiqueId, AnneeAcademiqueLibelle, Statut, DateInscription);
