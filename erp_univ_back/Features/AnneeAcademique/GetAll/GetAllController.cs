using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.AnneeAcademique.GetAll
{
    [ApiController]
    [Route("api/annees-academiques")]
    public class GetAllController(IGetAllService _service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(List<AnneeAcademiqueGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllAnneeAcademiqueRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(request, ct);
            return Ok(response);
        }
    }
}
