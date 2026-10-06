using erp_univ_back.Features.Etablissement.Create;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etablissement.CreateEtablissement;

[ApiController]
[Route("api/etablissements")]
public class CreateController(ICreateService _createService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(EtablissementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEtablissement([FromBody] EtablissementRequest request)
    {
        var response = await _createService.ExecuteAsync(request);
        return CreatedAtAction(nameof(CreateEtablissement), new { id = response.Id }, response);
    }
}
