namespace erp_univ_back.Features.Niveau.Update;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/niveaux")]
public class UpdateController(IUpdateNiveauService service) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(NiveauResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateNiveauRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}
