namespace erp_univ_back.Features.AnneeAcademique.GetActive
{
    public record GetActiveAnneeAcademiqueResponse(
        Guid Id,
        string Libelle,
        DateOnly DateDebut,
        DateOnly DateFin,
        bool EstActive,
        DateTime DateCreation,
        DateTime DateModification
    );
}
