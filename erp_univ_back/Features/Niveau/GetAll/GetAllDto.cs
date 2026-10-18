namespace erp_univ_back.Features.Niveau.GetAll;

public record NiveauResponse(
    Guid Id,
    string Code,
    string Nom,
    int Ordre
);
