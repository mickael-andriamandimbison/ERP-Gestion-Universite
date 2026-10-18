namespace erp_univ_back.Features.FiliereNiveau.Create
{
    public record CreateFiliereNiveauRequest(
    Guid FiliereId,
    Guid NiveauId,
    string? LibelleAffiche
);

    public record CreateFiliereNiveauResponse(
        Guid Id,
        Guid FiliereId,
        string FiliereCode,
        string FiliereNom,
        Guid NiveauId,
        string NiveauCode,
        string NiveauNom,
        string LibelleAffiche,
        DateTime DateCreation
    );
}