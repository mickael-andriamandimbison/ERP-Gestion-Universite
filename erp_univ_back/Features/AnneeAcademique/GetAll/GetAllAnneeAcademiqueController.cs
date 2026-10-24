namespace erp_univ_back.Features.AnneeAcademique.GetAll;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/annees-academiques")]
public class GetAllAnneeAcademiqueController(IGetAllAnneeAcademiqueService _service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<GetAllAnneeAcademiqueResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllAnneeAcademiqueRequest request, CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(request, ct);
        return Ok(response);
    }
}
