namespace erp_univ_back.Features.Matiere.Update;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matieres")]
public class UpdateMatiereController(IUpdateMatiereService service) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateMatiereResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> MettreAJour([FromRoute] Guid id, [FromBody] UpdateMatiereRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}

