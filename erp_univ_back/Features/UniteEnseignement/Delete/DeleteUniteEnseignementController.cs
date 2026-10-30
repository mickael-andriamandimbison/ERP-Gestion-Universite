namespace erp_univ_back.Features.UniteEnseignement.Delete;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/unites-enseignement")]
public class DeleteUniteEnseignementController(IDeleteUniteEnseignementService service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Supprimer([FromRoute] Guid id, CancellationToken ct)
    {
        await service.ExecuteAsync(id, ct);
        return NoContent();
    }
}

