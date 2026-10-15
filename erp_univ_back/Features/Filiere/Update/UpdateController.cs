using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Filiere.Update
{
    [ApiController]
    [Route("api/filieres")]
    public class UpdateController(IUpdateService _service) : ControllerBase
    {
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UpdateFiliereResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFiliereRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(id, request, ct);
            return Ok(response);
        }
    }
}
