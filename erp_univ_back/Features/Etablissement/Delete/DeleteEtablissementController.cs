namespace erp_univ_back.Features.Etablissement.Delete;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/etablissements")]
public class DeleteEtablissementController(IDeleteEtablissementService _service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        await _service.ExecuteAsync(id, ct);
        return Ok(new { message = "Etablissement supprimé avec succès" });
    }
}
