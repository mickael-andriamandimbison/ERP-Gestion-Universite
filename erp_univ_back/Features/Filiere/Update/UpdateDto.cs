namespace erp_univ_back.Features.Filiere.Update
{
    public record UpdateFiliereRequest(
        string Code,
        string Nom,
        string? Description = null
    );

    public record UpdateFiliereResponse(
        Guid Id,
        string Code,
        string Nom,
        string? Description,
        DateTime DateModification
    );
}
