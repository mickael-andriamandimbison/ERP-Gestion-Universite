using Microsoft.AspNetCore.Mvc;

namespace erp_univ_back.Features.Filiere.Create
{
    [ApiController]
    [Route("api/filieres")]
    public class CreateController(ICreateService _service) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateFiliereResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateFiliereRequest request, CancellationToken ct)
        {
            var response = await _service.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
        }
    }
}
