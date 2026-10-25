using erp_univ_back.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Professeur.GetAll;

[ApiController]
[Route("api/professeurs")]
public class GetAllProfesseurController(IGetAllProfesseurService _service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<GetAllProfesseurResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllProfesseurRequest request, CancellationToken ct)
    {
        var result = await _service.ExecuteAsync(request, ct);
        return Ok(result);
    }
}
