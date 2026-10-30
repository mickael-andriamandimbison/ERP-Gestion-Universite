namespace erp_univ_back.Features.Matiere.Create;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matieres")]
public class CreateMatiereController(ICreateMatiereService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateMatiereResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Creer([FromBody] CreateMatiereRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(Creer), new { id = response.Id }, response);
    }
}
