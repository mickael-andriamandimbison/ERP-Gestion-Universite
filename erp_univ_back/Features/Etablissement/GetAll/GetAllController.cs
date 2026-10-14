using erp_univ_back.Features.Etablissement.Create;
using erp_univ_back.Shared.Responses;
using Microsoft.AspNetCore.Mvc;


namespace erp_univ_back.Features.Etablissement.GetAll
{
    [ApiController]
    [Route("api/etablissements")]
    public class GetAllController(IGetAllService _service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<EtablissementResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllEtablissementsRequest request, CancellationToken ct)
        {
            var result = await _service.ExecuteAsync(request, ct);
            return Ok(result);
        }
    }
}
