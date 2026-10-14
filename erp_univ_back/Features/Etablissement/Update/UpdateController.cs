using Microsoft.AspNetCore.Mvc;
using erp_univ_back.Features.Etablissement.Create;

namespace erp_univ_back.Features.Etablissement.Update
{
    [Route("api/etablissements")]
    [ApiController]
    public class UpdateController(IUpdateService updateService) : ControllerBase
    {
        [HttpPut("{id}")]
        public async Task<ActionResult<EtablissementResponse>> UpdateAsync(Guid id, [FromBody] EtablissementRequest request, CancellationToken ct)
        {
            var response = await updateService.ExecuteAsync(id, request, ct);
            return Ok(response);
        }
    }
}
