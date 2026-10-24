namespace erp_univ_back.Features.AnneeAcademique.Update;

public record UpdateAnneeAcademiqueRequest(
    string Libelle,
    DateOnly DateDebut,
    DateOnly DateFin
);

public record UpdateAnneeAcademiqueResponse(
    Guid Id,
    string Libelle,
    DateOnly DateDebut,
    DateOnly DateFin,
    bool EstActive,
    DateTime DateModification
);
