using erp_univ_back.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Professeur.GetAll
{
    [ApiController]
    [Route("api/professeurs")]
    public class GetAllController(IGetAllProfesseurService _service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<ProfesseurResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProfesseursRequest request, CancellationToken ct)
        {
            var result = await _service.ExecuteAsync(request, ct);
            return Ok(result);
        }
    }
}
