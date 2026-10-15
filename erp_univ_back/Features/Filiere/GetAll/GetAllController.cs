using erp_univ_back.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Filiere.GetAll
{
    [ApiController]
    [Route("api/filieres")]
    public class GetAllController(IGetAllService _service) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<FiliereGetAllResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllFilieresRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(request, ct);
            return Ok(response);
        }
    }
}
