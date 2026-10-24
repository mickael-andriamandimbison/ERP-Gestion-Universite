namespace erp_univ_back.Features.Etablissement.Create;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/etablissements")]
public class CreateEtablissementController(ICreateEtablissementService _createService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateEtablissementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEtablissement([FromBody] CreateEtablissementRequest request, CancellationToken ct)
    {
        var response = await _createService.ExecuteAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
