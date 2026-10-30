namespace erp_univ_back.Features.Matiere.Update;

public record UpdateMatiereRequest(
    string Nom,
    int Coefficient,
    string? Code = null,
    Guid? UniteEnseignementId = null
);

public record UpdateMatiereResponse(
    Guid Id,
    string Nom,
    string Code,
    int Coefficient,
    Guid UniteEnseignementId,
    DateTime DateModification
);

