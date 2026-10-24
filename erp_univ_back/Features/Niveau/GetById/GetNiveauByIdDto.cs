namespace erp_univ_back.Features.Niveau.GetById;

public record GetNiveauByIdResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre,
    DateTime DateCreation,
    DateTime DateModification
);

public record NiveauDetailResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre,
    DateTime DateCreation,
    DateTime DateModification
) : GetNiveauByIdResponse(Id, Code, Nom, Ordre, DateCreation, DateModification);
