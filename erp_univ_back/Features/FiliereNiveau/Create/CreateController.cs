namespace erp_univ_back.Features.FiliereNiveau.Create;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/filiere-niveaux")]
public class CreateController(ICreateFiliereNiveauService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateFiliereNiveauResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Creer([FromBody] CreateFiliereNiveauRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Creer), new { id = response.Id }, response);
    }
}
