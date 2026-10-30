namespace erp_univ_back.Features.Matiere.GetPaginated;

using erp_univ_back.Features.Matiere.GetByUeId;
using erp_univ_back.Shared.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matieres")]
public class GetMatieresPaginatedController(IGetMatieresPaginatedService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<MatiereItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenirPagine([FromQuery] GetMatieresPaginatedRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return Ok(response);
    }
}

