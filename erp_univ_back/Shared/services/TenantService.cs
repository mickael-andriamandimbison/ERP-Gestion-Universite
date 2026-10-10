namespace erp_univ_back.Shared.Services
{
    public interface ITenantService
    {
        Guid? EtablissementId { get; }
    }

    public class TenantService(IHttpContextAccessor _httpContextAccessor) : ITenantService
    {
        public Guid? EtablissementId
        {
            get
            {
                var header = _httpContextAccessor.HttpContext?.Request.Headers["X-EtablissementId"].FirstOrDefault();
                if (string.IsNullOrEmpty(header) || !Guid.TryParse(header, out var id))
                {
                    throw new UnauthorizedAccessException("Etablissement non trouvé");
                }
                return id;
            }
        }
    }
}