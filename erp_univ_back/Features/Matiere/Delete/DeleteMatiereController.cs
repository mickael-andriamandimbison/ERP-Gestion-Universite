namespace erp_univ_back.Features.Matiere.Delete;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/matieres")]
public class DeleteMatiereController(IDeleteMatiereService service) : ControllerBase
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

