namespace erp_univ_back.Features.Niveau.GetAll;

public record GetAllNiveauResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre
);

public record NiveauResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre
) : GetAllNiveauResponse(Id, Code, Nom, Ordre);
