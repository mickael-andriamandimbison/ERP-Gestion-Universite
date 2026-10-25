namespace erp_univ_back.Features.Filiere.Create;

public record CreateFiliereRequest(
    string Code,
    string Nom,
    string? Description = null
);

public record CreateFiliereResponse(
    Guid Id,
    string Code,
    string Nom,
    string? Description,
    DateTime DateCreation
);
