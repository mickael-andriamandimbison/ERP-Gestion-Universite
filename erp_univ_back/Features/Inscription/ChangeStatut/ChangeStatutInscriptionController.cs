namespace erp_univ_back.Features.Inscription.ChangeStatut;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inscriptions")]
public class ChangeStatutInscriptionController(IChangeStatutInscriptionService service) : ControllerBase
{
    [HttpPatch("{id:guid}/statut")]
    [ProducesResponseType(typeof(ChangeStatutInscriptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatutAsync(
        [FromRoute] Guid id,
        [FromBody] ChangeStatutInscriptionRequest request,
        CancellationToken ct)
    {
        var response = await service.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}
