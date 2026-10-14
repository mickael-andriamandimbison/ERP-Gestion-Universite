using erp_univ_back.Shared.Services;

namespace erp_univ_back.Shared.Middlewares
{
    public class TenantMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            var claimEtablissement = context.User.FindFirst("etablissement_id")?.Value;

            if (Guid.TryParse(claimEtablissement, out var idFromClaim))
            {
                tenantService.DefinirEtablissement(idFromClaim);
            }
            else if (context.Request.Headers.TryGetValue("X-EtablissementId", out var headerValue)
                     && Guid.TryParse(headerValue.FirstOrDefault(), out var idFromHeader))
            {
                tenantService.DefinirEtablissement(idFromHeader);
            }

            await next(context);
        }
    }
}