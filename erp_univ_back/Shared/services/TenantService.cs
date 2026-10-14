namespace erp_univ_back.Shared.Services;

public interface ITenantService
{
    Guid? EtablissementId { get; }
    void DefinirEtablissement(Guid etablissementId);
}

public class TenantService : ITenantService
{
    public Guid? EtablissementId { get; private set; }

    public void DefinirEtablissement(Guid etablissementId)
    {
        EtablissementId = etablissementId;
    }
}