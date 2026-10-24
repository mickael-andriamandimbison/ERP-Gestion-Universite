namespace erp_univ_back.Features.FiliereNiveau.GetByFiliere;

public record GetNiveauxByFiliereResponse(
    Guid Id,
    Guid NiveauId,
    string NiveauCode,
    string NiveauNom,
    int Ordre,
    string LibelleAffiche
);
