namespace erp_univ_back.Features.AnneeAcademique.SetActive
{
    public record SetActiveAnneeAcademiqueResponse(
        Guid Id,
        string Libelle,
        DateOnly DateDebut,
        DateOnly DateFin,
        bool EstActive,
        string Message
    );
}
