using erp_univ_back.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etudiant.GetAll
{
    [ApiController]
    [Route("api/etudiants")]
    public class GetAllController(IGetAllEtudiantService _service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<EtudiantGetAllResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllEtudiantRequest request, CancellationToken ct)
        {
            var result = await _service.ExecuteAsync(request, ct);
            return Ok(result);
        }
    }
}