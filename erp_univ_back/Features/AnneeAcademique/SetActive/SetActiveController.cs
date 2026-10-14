using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.AnneeAcademique.SetActive
{
    [ApiController]
    [Route("api/annees-academiques")]
    public class SetActiveController(ISetActiveService _service) : ControllerBase
    {
        [HttpPut("{id:guid}/set-active")]
        [ProducesResponseType(typeof(SetActiveAnneeAcademiqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SetActive([FromRoute] Guid id, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(id, ct);
            return Ok(response);
        }
    }
}
