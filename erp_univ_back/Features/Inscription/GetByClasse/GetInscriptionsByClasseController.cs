namespace erp_univ_back.Features.Inscription.GetByClasse;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inscriptions")]
public class GetInscriptionsByClasseController(IGetInscriptionsByClasseService service) : ControllerBase
{
    [HttpGet("classe/{filiereNiveauId:guid}/annee/{anneeAcademiqueId:guid}")]
    [ProducesResponseType(typeof(List<GetInscriptionsByClasseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByClasseAsync(
        [FromRoute] Guid filiereNiveauId,
        [FromRoute] Guid anneeAcademiqueId,
        CancellationToken ct)
    {
        var response = await service.ExecuteAsync(filiereNiveauId, anneeAcademiqueId, ct);
        return Ok(response);
    }
}
