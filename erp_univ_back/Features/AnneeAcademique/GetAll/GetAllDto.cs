namespace erp_univ_back.Features.AnneeAcademique.GetAll
{
    public record GetAllAnneeAcademiqueRequest(
        string? Recherche = null,
        bool? EstActive = null
    );

    public record AnneeAcademiqueGetAllResponse(
        Guid Id,
        string Libelle,
        DateOnly DateDebut,
        DateOnly DateFin,
        bool EstActive,
        DateTime DateCreation,
        DateTime DateModification
    );
}
