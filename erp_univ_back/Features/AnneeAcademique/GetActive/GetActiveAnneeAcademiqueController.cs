namespace erp_univ_back.Features.AnneeAcademique.GetActive;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/annees-academiques")]
public class GetActiveAnneeAcademiqueController(IGetActiveAnneeAcademiqueService _service) : ControllerBase
{
    [HttpGet("active")]
    [ProducesResponseType(typeof(GetActiveAnneeAcademiqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(ct);
        return Ok(response);
    }
}
