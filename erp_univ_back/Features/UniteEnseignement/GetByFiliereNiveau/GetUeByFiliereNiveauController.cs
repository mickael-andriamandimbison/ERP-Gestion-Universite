namespace erp_univ_back.Features.UniteEnseignement.GetByFiliereNiveau;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/unites-enseignement")]
public class GetUeByFiliereNiveauController(IGetUeByFiliereNiveauService service) : ControllerBase
{
    [HttpGet]
    [HttpGet("by-filiere-niveau")]
    [ProducesResponseType(typeof(GetUeByFiliereNiveauResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenirParFiliereNiveau([FromQuery] GetUeByFiliereNiveauRequest request, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(request, ct);
        return Ok(response);
    }
}

