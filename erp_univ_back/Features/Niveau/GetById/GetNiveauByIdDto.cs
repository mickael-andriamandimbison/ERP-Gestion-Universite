namespace erp_univ_back.Features.Niveau.GetById;

public record GetNiveauByIdResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre,
    DateTime DateCreation,
    DateTime DateModification
);
