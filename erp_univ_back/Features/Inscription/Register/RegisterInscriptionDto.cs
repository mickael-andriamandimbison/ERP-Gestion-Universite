using erp_univ_back.Features.Etudiant.Create;

namespace erp_univ_back.Features.Inscription.Register;
public record RegisterInscriptionRequest(

    CreateEtudiantRequest EtudiantInfos,
    Guid IdFiliereNiveau,
    Guid IdAnneeAcademique

);

public record RegisterInscriptionResponse(
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