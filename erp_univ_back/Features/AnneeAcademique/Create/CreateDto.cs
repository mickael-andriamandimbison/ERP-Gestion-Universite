namespace erp_univ_back.Features.AnneeAcademique.Create
{
    public record CreateAnneeAcademiqueRequest(
        string Libelle,
        DateOnly DateDebut,
        DateOnly DateFin,
        bool EstActive = false
    );

    public record CreateAnneeAcademiqueResponse(
        Guid Id,
        string Libelle,
        DateOnly DateDebut,
        DateOnly DateFin,
        bool EstActive,
        DateTime DateCreation
    );
}
