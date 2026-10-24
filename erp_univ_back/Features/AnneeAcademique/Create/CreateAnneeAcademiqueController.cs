namespace erp_univ_back.Features.AnneeAcademique.Create;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/annees-academiques")]
public class CreateAnneeAcademiqueController(ICreateAnneeAcademiqueService _service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateAnneeAcademiqueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateAnneeAcademiqueRequest request, CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
