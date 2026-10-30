using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etudiant.Create;

[ApiController]
[Route("api/etudiants")]
public class CreateEtudiantController(ICreateEtudiantService _createService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateEtudiantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEtudiant([FromBody] CreateEtudiantRequest request, CancellationToken ct)
    {
        var response = await _createService.ExecuteAsync(request, ct);
        return CreatedAtAction(nameof(CreateEtudiant), new { id = response.Id }, response);
    }
}
