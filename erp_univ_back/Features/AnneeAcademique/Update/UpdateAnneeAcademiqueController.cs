namespace erp_univ_back.Features.AnneeAcademique.Update;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/annees-academiques")]
public class UpdateAnneeAcademiqueController(IUpdateAnneeAcademiqueService _service) : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UpdateAnneeAcademiqueResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateAnneeAcademiqueRequest request, CancellationToken ct)
    {
        var response = await _service.ExecuteAsync(id, request, ct);
        return Ok(response);
    }
}
