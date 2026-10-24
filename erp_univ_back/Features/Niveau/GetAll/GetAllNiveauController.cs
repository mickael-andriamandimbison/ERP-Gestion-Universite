namespace erp_univ_back.Features.Niveau.GetAll;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/niveaux")]
public class GetAllNiveauController(IGetAllNiveauService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GetAllNiveauResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
        var response = await service.ExecuteAsync(ct);
        return Ok(response);
    }
}
