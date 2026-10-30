namespace erp_univ_back.Features.UniteEnseignement.GetById;

public record GetUniteEnseignementByIdResponse(
    Guid Id,
    string Code,
    string Nom,
    int Credit,
    int Semestre,
    Guid FiliereNiveauId,
    string FiliereNiveauLibelle,
    Guid FiliereId,
    string FiliereNom,
    Guid NiveauId,
    string NiveauNom,
    string NiveauCode,
    DateTime DateCreation
);

