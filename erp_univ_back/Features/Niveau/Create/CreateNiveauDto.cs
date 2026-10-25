namespace erp_univ_back.Features.Niveau.Create;

public record CreateNiveauRequest(
    string Code,
    string Nom,
    int Ordre
);

public record CreateNiveauResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre,
    DateTime DateCreation
);
