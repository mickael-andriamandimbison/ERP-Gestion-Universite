namespace erp_univ_back.Features.Niveau.Update;

public record UpdateNiveauRequest(
    string Code,
    string Nom,
    int Ordre
);

public record NiveauResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre
);
