using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Professeur.Create;

[ApiController]
[Route("api/professeurs")]
public class CreateProfesseurController(ICreateProfesseurService _service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateProfesseurResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProf([FromBody] CreateProfesseurRequest request, CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(CreateProf), new { id = response.Id }, response);
    }
}
