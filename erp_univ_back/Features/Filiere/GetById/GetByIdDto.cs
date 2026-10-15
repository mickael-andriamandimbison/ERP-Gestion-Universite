namespace erp_univ_back.Features.Filiere.GetById
{
    public record FiliereGetByIdResponse(
        Guid Id,
        string Code,
        string Nom,
        string? Description,
        DateTime DateCreation,
        DateTime DateModification
    );
}
