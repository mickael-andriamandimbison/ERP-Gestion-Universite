namespace erp_univ_back.Features.UniteEnseignement.Update;

public record UpdateUniteEnseignementRequest(
    string Nom,
    int Credit,
    int Semestre,
    string? Code = null,
    Guid? FiliereNiveauId = null
);

public record UpdateUniteEnseignementResponse(
    Guid Id,
    string Code,
    string Nom,
    int Credit,
    int Semestre,
    Guid FiliereNiveauId,
    string FiliereNiveauLibelle,
    DateTime DateModification
);

