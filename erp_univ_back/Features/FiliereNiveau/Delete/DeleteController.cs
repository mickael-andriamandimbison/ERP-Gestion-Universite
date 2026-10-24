namespace erp_univ_back.Features.FiliereNiveau.Delete;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/filiere-niveaux")]
public class DeleteController(IDeleteFiliereNiveauService service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken ct)
    {
        await service.ExecuteAsync(id, ct);
        return NoContent();
    }
}
