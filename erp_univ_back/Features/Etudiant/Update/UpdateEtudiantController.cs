using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Etudiant.Update;

[ApiController]
[Route("api/etudiants")]
public class UpdateEtudiantController(IUpdateEtudiantService _service) : ControllerBase
{
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UpdateEtudiantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEtudiantRequest request, CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}
