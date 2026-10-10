using Microsoft.AspNetCore.Mvc;


namespace erp_univ_back.Features.Etablissement.GetAll
{
    [ApiController]
    [Route("api/etablissements")]
    public class GetAllController(IGetAllService _service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllEtablissementsRequest request, CancellationToken ct)
        {
            var result = await _service.ExecuteAsync(request, ct);
            return Ok(result);
        }
    }
}
