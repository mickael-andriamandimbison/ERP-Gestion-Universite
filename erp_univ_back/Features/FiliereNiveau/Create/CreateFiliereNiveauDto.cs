namespace erp_univ_back.Features.FiliereNiveau.Create;

public record CreateFiliereNiveauRequest(
    Guid FiliereId,
    Guid NiveauId
);

public record CreateFiliereNiveauResponse(
    Guid Id,
    Guid FiliereId,
    string FiliereNom,
    Guid NiveauId,
    string NiveauNom,
    string LibelleAffiche,
    DateTime DateCreation
);
