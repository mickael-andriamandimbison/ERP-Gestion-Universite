namespace erp_univ_back.Features.UniteEnseignement.GetById;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/unites-enseignement")]
public class GetUniteEnseignementByIdController(IGetUniteEnseignementByIdService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GetUniteEnseignementByIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenirParId([FromRoute] Guid id, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, ct);
        return Ok(response);
    }
}

