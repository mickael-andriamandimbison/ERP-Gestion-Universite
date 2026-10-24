namespace erp_univ_back.Features.Niveau.Delete;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/niveaux")]
public class DeleteController(IDeleteNiveauService service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken ct)
    {
        await service.ExecuteAsync(id, ct);
        return NoContent();
    }
}
