namespace erp_univ_back.Features.FiliereNiveau.GetAll;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/filiere-niveaux")]
public class GetAllFiliereNiveauController(IGetAllFiliereNiveauService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<FiliereNiveauDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var response = await service.ExecuteAsync(ct);
        return Ok(response);
    }
}
