namespace erp_univ_back.Shared.Services;

using erp_univ_back.Shared.Exceptions;

public interface ITenantService
{
    Guid? EtablissementId { get; }

    void DefinirEtablissement(Guid etablissementId);

    Guid ObtenirEtablissementId();
}

public class TenantService : ITenantService
{
    public Guid? EtablissementId { get; private set; }

    public void DefinirEtablissement(Guid etablissementId)
    {
        if (etablissementId == Guid.Empty)
        {
            throw new BadRequestException("L'identifiant de l'établissement ne peut pas être un GUID vide.");
        }

        EtablissementId = etablissementId;
    }

    public Guid ObtenirEtablissementId()
    {
        if (!EtablissementId.HasValue || EtablissementId.Value == Guid.Empty)
        {
            throw new UnauthorizedException("Aucun établissement n'a été défini dans le contexte de la requête.");
        }

        return EtablissementId.Value;
    }
}