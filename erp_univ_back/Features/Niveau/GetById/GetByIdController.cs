namespace erp_univ_back.Features.Niveau.GetById;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/niveaux")]
public class GetNiveauByIdController(IGetNiveauByIdService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NiveauDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, ct);
        return Ok(response);
    }
}
