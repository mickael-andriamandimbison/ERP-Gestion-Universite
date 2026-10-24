namespace erp_univ_back.Features.Inscription.GetByEtudiant;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/inscriptions")]
public class GetInscriptionsByEtudiantController(IGetInscriptionsByEtudiantService service) : ControllerBase
{
    [HttpGet("etudiant/{etudiantId:guid}")]
    [ProducesResponseType(typeof(List<InscriptionByEtudiantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEtudiantAsync([FromRoute] Guid etudiantId, CancellationToken ct)
    {
        var response = await service.ExecuteAsync(etudiantId, ct);
        return Ok(response);
    }
}
