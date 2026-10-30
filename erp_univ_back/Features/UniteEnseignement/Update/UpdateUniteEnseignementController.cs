namespace erp_univ_back.Features.UniteEnseignement.Update;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/unites-enseignement")]
public class UpdateUniteEnseignementController(IUpdateUniteEnseignementService service) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateUniteEnseignementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MettreAJour([FromRoute] Guid id, [FromBody] UpdateUniteEnseignementRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}

