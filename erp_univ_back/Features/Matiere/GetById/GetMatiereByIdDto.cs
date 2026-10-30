namespace erp_univ_back.Features.Matiere.GetById;

public record GetMatiereByIdResponse(
    Guid Id,
    string Nom,
    string Code,
    int Coefficient,
    Guid UniteEnseignementId,
    string UniteEnseignementNom,
    string UniteEnseignementCode,
    Guid FiliereId,
    string FiliereNom,
    Guid NiveauId,
    string NiveauNom,
    DateTime DateCreation
);

