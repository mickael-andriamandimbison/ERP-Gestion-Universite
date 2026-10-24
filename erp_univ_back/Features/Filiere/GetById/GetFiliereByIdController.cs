using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Filiere.GetById;

[ApiController]
[Route("api/filieres")]
public class GetFiliereByIdController(IGetFiliereByIdService _service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetFiliereByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(id, ct);
        return Ok(response);
    }
}
