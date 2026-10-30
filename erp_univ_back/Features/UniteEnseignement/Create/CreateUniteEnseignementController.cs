namespace erp_univ_back.Features.UniteEnseignement.Create;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/unites-enseignement")]
public class CreateUniteEnseignementController(ICreateUniteEnseignementService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateUniteEnseignementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Creer([FromBody] CreateUniteEnseignementRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Creer), new { id = response.Id }, response);
    }
}

