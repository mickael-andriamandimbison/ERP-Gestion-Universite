using Microsoft.AspNetCore.Mvc;
using erp_univ_back.Features.Etablissement.Create;

namespace erp_univ_back.Features.Etablissement.Update;

[Route("api/etablissement")]
[ApiController]
public class UpdateController(IUpdateService updateService) : ControllerBase
{
    [HttpPut("{id}")]
    public async Task<ActionResult<EtablissementResponse>> UpdateAsync(Guid id, EtablissementRequest request)
    {
        var response = await updateService.ExecuteAsync(id, request);
        return Ok(response);
    }
}