namespace erp_univ_back.Features.Matiere.Create;

public record CreateMatiereRequest(
    string Nom,
    string Code,
    int Coefficient,
    Guid UniteEnseignementId
);

public record CreateMatiereResponse(
    Guid Id,
    string Nom,
    string Code,
    int Coefficient,
    Guid UniteEnseignementId,
    DateTime DateCreation
);