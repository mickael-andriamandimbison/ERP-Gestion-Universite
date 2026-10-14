using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.AnneeAcademique.Create
{
    [ApiController]
    [Route("api/annees-academiques")]
    public class CreateController(ICreateService _service) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateAnneeAcademiqueResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateAnneeAcademiqueRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
        }
    }
}
