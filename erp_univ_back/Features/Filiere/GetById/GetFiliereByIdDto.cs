namespace erp_univ_back.Features.Filiere.GetById;

public record GetFiliereByIdResponse(
    Guid Id,
    string Code,
    string Nom,
    string? Description,
    DateTime DateCreation,
    DateTime DateModification
);

public record FiliereGetByIdResponse(
    Guid Id,
    string Code,
    string Nom,
    string? Description,
    DateTime DateCreation,
    DateTime DateModification
) : GetFiliereByIdResponse(Id, Code, Nom, Description, DateCreation, DateModification);
