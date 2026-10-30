namespace erp_univ_back.Features.UniteEnseignement.Create;

public record CreateUniteEnseignementRequest(
    string Nom,
    int Credit,
    int Semestre,
    Guid FiliereNiveauId,
    string? Code = null
);

public record CreateUniteEnseignementResponse(
    Guid Id,
    string Code,
    string Nom,
    int Credit,
    int Semestre,
    Guid FiliereNiveauId,
    string FiliereNiveauLibelle,
    DateTime DateCreation
);

