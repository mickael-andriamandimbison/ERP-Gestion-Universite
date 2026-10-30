namespace erp_univ_back.Features.Matiere.GetByUeId;

public record GetMatieresByUeIdRequest(Guid UeId);

public record MatiereItemDto(
    Guid Id,
    string Code,
    string Nom,
    int Coefficient
);

