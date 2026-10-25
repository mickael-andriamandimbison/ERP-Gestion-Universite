namespace erp_univ_back.Features.FiliereNiveau.GetByFiliere;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/filiere-niveaux")]
public class GetNiveauxByFiliereController(IGetNiveauxByFiliereService service) : ControllerBase
{
    [HttpGet("by-filiere/{filiereId:guid}")]
    [ProducesResponseType(typeof(List<GetNiveauxByFiliereResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByFiliereAsync([FromRoute] Guid filiereId, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(filiereId, ct);
        return Ok(response);
    }
}
