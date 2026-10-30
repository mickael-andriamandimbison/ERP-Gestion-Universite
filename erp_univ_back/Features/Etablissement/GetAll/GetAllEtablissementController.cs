namespace erp_univ_back.Features.Etablissement.GetAll;

using erp_univ_back.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/etablissements")]
public class GetAllEtablissementController(IGetAllEtablissementService _service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<GetAllEtablissementResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllEtablissementsRequest request, CancellationToken ct)
    {
        var result = await _service.ExecuteAsync(request, ct);
        return Ok(result);
    }
}
